using Scalar.AspNetCore;
using Thrume.Api.Extensions;
using Thrume.Domain.Entity;
using Thrume.Infrastructure;
using Thrume.Services;
using Thrume.Api.Endpoints; 
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;


var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ServiceProviderOptions>(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.AddEmailSender()
    .AddIdentity()
    .AddDatabase()
    //.AddAuth()
    .AddMinio();
builder.Services.AddAuthentication().AddCookie("Identity.Bearer");
builder.Services.AddScoped<IFileStorageRepository, ImageStorageRepository>();
builder.Services.AddTransient<AccountService>();
builder.Services.AddTransient<CommentService>();
builder.Services.AddTransient<PostService>();
builder.Services.AddTransient<MessageService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});
// builder.Services.ConfigureApplicationCookie(opts =>
// {
//     opts.Cookie.SameSite = SameSiteMode.None;
//     opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//     opts.Events.OnRedirectToLogin = ctx =>
//     {
//         if (ctx.Request.Path.StartsWithSegments("/account"))
//             ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
//         else
//             ctx.Response.Redirect(ctx.RedirectUri);
//         return Task.CompletedTask;
//     };
//     opts.Events.OnRedirectToAccessDenied = ctx =>
//     {
//         ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
//         return Task.CompletedTask;
//     };
// });

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN"; 
    options.Cookie.Name = "MYAPP-XSRF-TOKEN"; 
    options.Cookie.HttpOnly = false;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
    options.Cookie.SameSite = SameSiteMode.None;
});

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

var authGroup = app.MapGroup("/auth");


authGroup.MapPost("/logout", async (SignInManager<Account> signInManager, HttpContext httpContext) =>
{

    if (signInManager.IsSignedIn(httpContext.User))
    {
        await signInManager.SignOutAsync();

    }

    return Results.Ok(); 
});

app.MapGroup("/auth").MapIdentityApi<Account>();

app.MapGet("/antiforgery/token", (IAntiforgery antiforgery, HttpContext context) =>
    {
        var tokens = antiforgery.GetAndStoreTokens(context);
        return Results.Ok(new { requestToken = tokens.RequestToken });
    }).AllowAnonymous();


app.MapMiscEndpoints();
app.MapPostEndpoints();
app.MapAccountEndpoints();
app.MapCommentEndpoints();
app.MapMessageEndpoints();
app.MapGet("/", () => Results.Redirect("/scalar"));
app.Run();

static bool IsApiRequest(HttpRequest request)
{
    return request.Path.StartsWithSegments("/auth") ||
           request.Path.StartsWithSegments("/api") || 
           request.Path.StartsWithSegments("/account") || 
           (request.Headers.TryGetValue("Accept", out var acceptHeader) &&
            acceptHeader.Any(h => h != null && h.Contains("application/json", StringComparison.OrdinalIgnoreCase)));
}