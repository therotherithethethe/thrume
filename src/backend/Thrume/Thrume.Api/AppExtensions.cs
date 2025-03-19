using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Thrume.Database;
using Thrume.Domain;

namespace Thrume.Api;
public static class AppExtensions
{
    public static async Task<WebApplicationBuilder> ConfigureServices(this WebApplicationBuilder builder)
    {
        await builder.ConfigureEmailSender();
        builder.ConfigureIdentity();
        builder.ConfigureDatabase();
        builder.ConfigureAuth();
        return builder;
    }

    public static async Task<WebApplicationBuilder> ConfigureEmailSender(this WebApplicationBuilder builder)
    {
        await using var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);
        var credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.FromStream(stream).Secrets,
            ["https://mail.google.com/"],
            "wildchild250336@gmail.com",
            CancellationToken.None,
            new FileDataStore("token.json", true));
        if(credentials.Token.IsStale)
    {
        await credentials.RefreshTokenAsync(CancellationToken.None);
    }
        builder.Services.AddTransient<IEmailSender<Account>, EmailSender<Account>>(_ => new EmailSender<Account>(credentials));
        return builder;
    }

    public static WebApplicationBuilder ConfigureIdentity(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(opt =>
            opt.FallbackPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build());

        builder.Services.AddOpenApi();
        builder.Services.AddIdentity<Account, IdentityRole<AccountId>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

        builder.Services.Configure<IdentityOptions>(o =>
        {
            o.Password.RequireDigit = false;
            o.Password.RequireLowercase = false;
            o.Password.RequireUppercase = false;
            o.Password.RequireNonAlphanumeric = false;
            o.User.RequireUniqueEmail = true;
        });
        
        return builder;
    }

    public static WebApplicationBuilder ConfigureDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(o =>
            o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        return builder;
    }

    public static WebApplicationBuilder ConfigureAuth(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(y =>
            {
                y.SaveToken = false;
                y.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateLifetime = true,
                };
            })
            .AddCookie("Identity.Bearer");
        
        return builder;
    }
}