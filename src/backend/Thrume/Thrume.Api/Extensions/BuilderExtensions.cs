using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Thrume.Configuration;
using Thrume.Database;
using Thrume.Domain;
using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;
using Thrume.Infrastructure;
using Thrume.Services;

namespace Thrume.Api.Extensions;

public class AppRoles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Banned  = "Banned";
    public static string[] Roles => [Admin, User, Banned];
}

internal static class BuilderExtensions
{
    //TODO: refactor that shit
    public static WebApplicationBuilder AddEmailSender(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IEmailSender<Account>, IdentityEmailSender<Account>>();
        return builder;
    }

    public static async Task CheckRoles(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<AccountId>>>();
        foreach (var role in AppRoles.Roles)
        {
            var roleExist = await roleManager.RoleExistsAsync(role);
            if (!roleExist)
            {
                // === ОСЬ ВИРІШЕННЯ ===
                // Створюємо роль і ВРУЧНУ призначаємо їй новий, унікальний ID
                var newRole = new IdentityRole<AccountId>(role)
                {
                    Id = new AccountId(Guid.CreateVersion7())
                };
                
                // Тепер передаємо в CreateAsync об'єкт з уже встановленим ID
                await roleManager.CreateAsync(newRole);
            }
        }
    }
    public static WebApplicationBuilder AddIdentity(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            // Політика для адмінів. Залишається без змін.
            // Адміни не повинні бути забанені.
            options.AddPolicy("AdminOnly", policy =>
            {
                policy.RequireRole(AppRoles.Admin);
                policy.AddRequirements(new NotBannedRequirement());
            });

            // Політика для дій, які вимагають активного, незабаненого акаунту.
            // Наприклад, створення поста, коментаря.
            options.AddPolicy("StandardAccess", policy =>
            {
                policy.RequireAuthenticatedUser(); // Користувач має бути залогінений
                policy.AddRequirements(new NotBannedRequirement()); // І не забанений
            });
    
            // Політика для перегляду контенту.
            // Доступ має будь-який залогінений користувач, незалежно від статусу бану.
            options.AddPolicy("ViewContent", policy =>
            {
                policy.RequireAuthenticatedUser(); // Лише вимога бути залогіненим
            });
        });

        builder.Services.AddOpenApi();
        builder.Services.AddIdentity<Account, IdentityRole<AccountId>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
           

        builder.Services.Configure<IdentityOptions>(o =>
        {
            o.Password.RequireDigit = false;
            o.Password.RequireLowercase = false;
            o.Password.RequireUppercase = false;
            o.Password.RequireNonAlphanumeric = false;
            o.User.RequireUniqueEmail = true;
            o.SignIn.RequireConfirmedEmail = true;
        });
        
        return builder;
    }

    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(o =>
        {
            o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
            o.UseSnakeCaseNamingConvention();
        });
        return builder;
    }

    public static WebApplicationBuilder AddAuth(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<JwtConfiguration>()
            .BindConfiguration(nameof(JwtConfiguration))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            })
            .AddCookie(IdentityConstants.ApplicationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, y =>
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
            });

        return builder;
    }

    public static WebApplicationBuilder AddMinio1(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<MinioConfiguration>()
            .BindConfiguration(nameof(MinioConfiguration))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        builder.Services.AddScoped<IFileStorageRepository, ImageStorageRepository>();

        

        builder.Services.AddSingleton<IAmazonS3>(ImplementationFactory);

        return builder;
    }

    public static IServiceCollection AddSupabase(this IServiceCollection services)
    {
        // Біндимо нову конфігурацію
        services
            .AddOptions<SupabaseConfiguration>()
            .BindConfiguration("Supabase")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddSingleton(provider => 
        {
            var options = provider.GetRequiredService<IOptions<SupabaseConfiguration>>().Value;
        
            // Ініціалізація клієнта
            return new Supabase.Client(options.Url, options.Key, new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            });
        });

        // Реєструємо наш репозиторій для роботи зі сховищем
        services.AddScoped<IFileStorageRepository, ImageStorageRepository>();

        return services;
    }
    private static IAmazonS3 ImplementationFactory(IServiceProvider sp)
    {
        var minioSettings = sp.GetRequiredService<IOptions<MinioConfiguration>>().Value;
        Console.WriteLine(minioSettings.SecretKey);
        var config = new AmazonS3Config { ServiceURL = minioSettings.ServiceUrl, ForcePathStyle = true, AuthenticationRegion = "us-east-2"};
        var credentials = new BasicAWSCredentials(minioSettings.AccessKey, minioSettings.SecretKey);
        using var factory = LoggerFactory.Create(a => a.AddConsole());
        ILogger logger = factory.CreateLogger("Program");
        logger.LogError(minioSettings.ToString());
        return new AmazonS3Client(credentials, config);
        
    }
}