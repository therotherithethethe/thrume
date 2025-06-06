using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
internal static class BuilderExtensions
{
    //TODO: refactor that shit
    public static WebApplicationBuilder AddEmailSender(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IEmailSender<Account>, IdentityEmailSender<Account>>();
        return builder;
    }

    public static WebApplicationBuilder AddIdentity(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization();

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

    public static WebApplicationBuilder AddMinio(this WebApplicationBuilder builder)
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

    private static IAmazonS3 ImplementationFactory(IServiceProvider sp)
    {
        var minioSettings = sp.GetRequiredService<IOptions<MinioConfiguration>>().Value;
        Console.WriteLine(minioSettings.SecretKey);
        var config = new AmazonS3Config { ServiceURL = minioSettings.ServiceUrl, ForcePathStyle = true, UseHttp = !minioSettings.UseSsl };
        var credentials = new BasicAWSCredentials(minioSettings.AccessKey, minioSettings.SecretKey);
        return new AmazonS3Client(credentials, config);
    }
}