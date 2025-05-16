using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Thrume.Database;
using Thrume.Domain.EntityIds;
using Thrume.Services;

namespace Thrume.Api.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var accountGroup = app.MapGroup("/account").RequireAuthorization();


        accountGroup.MapPut("/updateProfile", async (IFormFile file,IAntiforgery antiforgery, HttpContext context, AccountService service) =>
        {
            await antiforgery.ValidateRequestAsync(context);
            var findFirst = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (findFirst is null)
            {
                return Results.Unauthorized();
            }

            await service.UpdateAvatarAsync(new AccountId(Guid.Parse(findFirst)), file);
            return Results.Ok();
        }).Accepts<IFormFile>("multipart/form-data");

        accountGroup.MapGet("/me", async (HttpContext context, AppDbContext dbContext, ILoggerFactory factory) =>
        {
            var value = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ILogger logger = factory.CreateLogger<Program>();
            logger.LogError("context claims {}", value);
            if (value is null) return Results.Unauthorized();
            if (!Guid.TryParse(value, out var guid))
            {
                return Results.Unauthorized();
            }

            var findAsync = await dbContext.AccountDbSet.FindAsync(new AccountId(guid));
            if (findAsync is null) return Results.Unauthorized();
            
            return Results.Ok(findAsync);
        });

        return app;
    }
}