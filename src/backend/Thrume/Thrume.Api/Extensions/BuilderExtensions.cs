using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
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
using Thrume.Services;
using Thrume.Services.Abstraction;

namespace Thrume.Api.Extensions;
internal static class BuilderExtensions
{
    //TODO: refactor that shit
    public static WebApplicationBuilder ConfigureEmailSender(this WebApplicationBuilder builder)
    {
        // const string user = "wildchild250336@gmail.com";
        // const string api = "https://mail.google.com/";
        // using var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);
        // var googleClientSecrets = GoogleClientSecrets.FromStream(stream);
        //
        // var credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(
        //     googleClientSecrets.Secrets,
        //     [api],
        //     user,
        //     CancellationToken.None,
        //     new FileDataStore("token.json", true)).GetAwaiter().GetResult();
        // if(credentials.Token.IsStale)
        // {
        //     credentials.RefreshTokenAsync(CancellationToken.None);
        //     ConfigureEmailSender(builder);
        //     return builder;
        // }
        builder.Services.AddTransient<IEmailSender<Account>, IdentityEmailSender<Account>>();
        return builder;
    }

    public static WebApplicationBuilder ConfigureIdentity(this WebApplicationBuilder builder)
    {
        // builder.Services.AddAuthorization(opt =>
        //     opt.FallbackPolicy = new AuthorizationPolicyBuilder()
        //         .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        //         .RequireAuthenticatedUser()
        //         .Build());
        builder.Services.AddAuthorization();

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
        {
            o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
            o.UseSnakeCaseNamingConvention();
        });
        return builder;
    }

    public static WebApplicationBuilder ConfigureAuth(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<JwtConfiguration>()
            .BindConfiguration(nameof(JwtConfiguration))
            .ValidateDataAnnotations()
            .ValidateOnStart();
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

    public static WebApplicationBuilder ConfigureMinio(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<MinioConfiguration>()
            .BindConfiguration(nameof(MinioConfiguration))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        builder.Services.AddScoped<IFileStorageService, MinioStorageService>();

        

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