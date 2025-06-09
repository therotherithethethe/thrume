using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Thrume.Api.Extensions;
using Thrume.Database;
using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;

namespace Thrume.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var adminGroup = app.MapGroup("/api/admin").RequireAuthorization("AdminOnly");

        adminGroup.MapPut("/change/{userName}", async
            (string userName, UserManager<Account> userManager, ChangePrivilegiesRequest req) =>
        {
            var account = await userManager.FindByNameAsync(userName);
            if (account == null) return Results.NotFound();
            
            await userManager.RemoveFromRoleAsync(account, AppRoles.User);
            await userManager.RemoveFromRoleAsync(account, AppRoles.Admin);
            await userManager.RemoveFromRoleAsync(account, AppRoles.Banned);
            
            if (!AppRoles.Roles.Contains(req.Role)) return Results.BadRequest();

            var result = await userManager.AddToRoleAsync(account, req.Role);
            if(req.NewNickname != null) await userManager.SetUserNameAsync(account, req.NewNickname);
            if(req.Email != null) await userManager.SetEmailAsync(account, req.Email);
            if (req.IsNeedToConfirmEmail && !await userManager.IsEmailConfirmedAsync(account))
            {
                var token = await userManager.GenerateEmailConfirmationTokenAsync(account);
                await userManager.ConfirmEmailAsync(account, token);
            }
            
            return result.Succeeded ? Results.Ok(account) : Results.BadRequest();
        });

        adminGroup.MapGet("/account", async (AppDbContext dbContext) =>
        {
            var usersWithRoles = await dbContext.Users
                .Select(user => new UserWithRolesDto
                (
                    user.Id,
                    user.UserName!,
                    user.PictureUrl,
                    user.EmailConfirmed,
                    // Побудова списку ролей через прямі JOIN'и
                    (from userRole in dbContext.UserRoles
                        where userRole.UserId.Equals(user.Id)
                        join role in dbContext.Roles
                            on userRole.RoleId equals role.Id
                        select role.Name).ToList(),
                    user.Email!
                ))
                .ToListAsync();

            return Results.Ok(usersWithRoles);
        });
        return app;
    }
}

public record UserWithRolesDto(AccountId Id, string UserName, string? PictureUrl, bool IsEmailConfirmed, IList<string> Roles, string Email);

public record ChangePrivilegiesRequest(string? Role, string? NewNickname = null, string? Email = null, bool IsNeedToConfirmEmail = false);