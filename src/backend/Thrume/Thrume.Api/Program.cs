using System.Security.Claims;
using Scalar.AspNetCore;
using Thrume.Api.Extensions;
using Thrume.Domain.Entity;
using Thrume.Infrastructure;
using Thrume.Services;
using Thrume.Api.Endpoints;
using Thrume.Api.Hubs;
using Thrume.Api.Filters;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Thrume.Api;
using Microsoft.AspNetCore.HttpOverrides;


var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ServiceProviderOptions>(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Render не використовує стандартний порт, тому не обмежуємо мережі
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Add Controllers
builder.Services.AddControllers();

builder.AddEmailSender()
    .AddIdentity()
    .AddDatabase();
    //.AddAuth()

    builder.Services.AddSupabase();
builder.Services.AddAuthentication().AddCookie("Identity.Bearer");
builder.Services.AddScoped<IAuthorizationHandler, NotBannedHandler>();
// SignalR Configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 32768; // 32KB max message size
    options.StreamBufferCapacity = 10;
    options.MaximumParallelInvocationsPerClient = 1;
});

// Register SignalR filter
builder.Services.AddScoped<SignalRRateLimitFilter>();

// Rate Limiting Configuration for SignalR (to be implemented)
// For now, rate limiting is handled in the SignalRRateLimitFilter

// Services
builder.Services.AddScoped<IFileStorageRepository, ImageStorageRepository>();
builder.Services.AddSingleton<IUserPresenceTracker, UserPresenceTracker>();
builder.Services.AddSingleton<ICallStateService, CallStateService>();
builder.Services.AddTransient<AccountService>();
builder.Services.AddTransient<CommentService>();
builder.Services.AddTransient<PostService>();
builder.Services.AddTransient<MessageService>();
builder.Services.AddHttpContextAccessor(); 

// Background services
builder.Services.AddHostedService<CallCleanupService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://thrume-frontend.onrender.com") // Your frontend's public URL
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});


// builder.Services.AddAntiforgery(options =>
// {
//     options.HeaderName = "X-XSRF-TOKEN"; 
//     options.Cookie.Name = "MYAPP-XSRF-TOKEN"; 
//     options.Cookie.HttpOnly = false;
//     options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
//     options.Cookie.SameSite = SameSiteMode.None;
// });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true; 
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
    options.Cookie.SameSite = SameSiteMode.None; 

    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true; 
    
    options.Events.OnRedirectToLogin = context =>
    {
        
        if (IsApiRequest(context.Request))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (IsApiRequest(context.Request))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };
});

var app = builder.Build();

app.UseExceptionHandler();
app.MapScalarApiReference();
app.MapOpenApi();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
//app.UseAntiforgery();
app.UseForwardedHeaders();
var authGroup = app.MapGroup("/api/auth");


authGroup.MapPost("/logout", async (SignInManager<Account> signInManager, HttpContext httpContext) =>
{

    if (signInManager.IsSignedIn(httpContext.User))
    {
        await signInManager.SignOutAsync();

    }

    return Results.Ok(); 
});
app.UseCors();
app.UseRouting(); 
app.MapGroup("/api/auth").MapIdentityApi<Account>();
app.MapGet("/api/auth/status", (ClaimsPrincipal claims) => 
    claims.Identity?.IsAuthenticated == true 
        ? Results.Ok(new { IsAuthenticated = true }) 
        : Results.Ok(new { IsAuthenticated = false })
        ).AllowAnonymous();
app.MapGet("/api/account/role", async (ClaimsPrincipal claimsPrincipal, UserManager<Account> userManager) =>
{
    var user = await userManager.GetUserAsync(claimsPrincipal);
    if (user == null)
    {
        // Ця ситуація малоймовірна, якщо є .RequireAuthorization(), але це надійна перевірка
        return Results.NotFound("User not found."); 
    }

    // 2. Отримуємо його ролі
    var roles = await userManager.GetRolesAsync(user);

    // 3. Повертаємо результат
    return Results.Ok(roles);
}).RequireAuthorization();


// app.MapGet("/antiforgery/token", (IAntiforgery antiforgery, HttpContext context) =>
//     {
//         var tokens = antiforgery.GetAndStoreTokens(context);
//         return Results.Ok(new { requestToken = tokens.RequestToken });
//     }).AllowAnonymous().DisableAntiforgery();


app.MapMiscEndpoints();
app.MapPostEndpoints();
app.MapAccountEndpoints();
app.MapCommentEndpoints();
app.MapMessageEndpoints();
app.MapAdminEndpoints();
app.MapGet("/health", () => Results.Ok("Healthy"));
// Map Call Controller
app.MapControllers();
app.MapHub<VoiceCallHub>("/voiceCallHub");
// SignalR Hub mapping
app.MapHub<ChatHub>("/chathub");

app.MapGet("/", () => Results.Redirect("/scalar"));
await app.CheckRoles();
app.Run();

static bool IsApiRequest(HttpRequest request)
{
    return request.Path.StartsWithSegments("/auth") ||
           request.Path.StartsWithSegments("/api") || 
           request.Path.StartsWithSegments("/account") || 
           (request.Headers.TryGetValue("Accept", out var acceptHeader) &&
            acceptHeader.Any(h => h != null && h.Contains("application/json", StringComparison.OrdinalIgnoreCase)));
}