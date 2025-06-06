using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Thrume.Database;
using Thrume.Domain.EntityIds;
using Thrume.Services;

namespace Thrume.Api.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var accountGroup = app.MapGroup("/account").RequireAuthorization();
        
        accountGroup.MapGet("/following/{userName}", async (ClaimsPrincipal claimsPrincipal, AppDbContext dbContext, string userName) =>
        {
            var currentUserId = GetCurrentUserId(claimsPrincipal);
            
            var listAsync = await dbContext
                .Subscriptions
                .Include(s => s.Follower)
                .Where(s => s.Follower.UserName == userName)
                .Select(s => new
                {
                    s.FollowingAccount.UserName,
                    s.FollowingAccount.PictureUrl
                }).ToListAsync();
            return Results.Ok(listAsync);
        }).AllowAnonymous();
        
        accountGroup.MapGet("/followers/{userName}", async (ClaimsPrincipal claimsPrincipal, AppDbContext dbContext, string userName) =>
        {
            var currentUserId = GetCurrentUserId(claimsPrincipal);

            var listAsync =
                await dbContext
                    .Subscriptions
                    .Include(s => s.FollowingAccount)
                    .Where(s => s.FollowingAccount.UserName == userName)
                    .Select(s => new
                    {
                        s.Follower.UserName,
                        s.Follower.PictureUrl
                    }).ToListAsync();
            return Results.Ok(listAsync);
        }).AllowAnonymous();
        
        accountGroup.MapGet("/profile/{userName}", async (HttpContext context, AppDbContext dbContext, string userName) =>
        {
            var currentUserId = GetCurrentUserId(context.User);
            
            var profile = await dbContext.AccountDbSet
                .Where(a => a.UserName == userName)
                .Select(a => new
                {
                    accountId = a.Id.Value,
                    userName = a.UserName,
                    pictureUrl = a.PictureUrl,
                    postCount = a.Posts.Count,
                    followersCount = a.Following.Count,
                    followingCount = a.Followers.Count,
                    amIFollowing = a.Following.Any(s => s.FollowerId == currentUserId)
                })
                .FirstOrDefaultAsync();
                
            if (profile is null) return Results.NotFound($"User {userName} not found");

            return Results.Ok(profile);
        }).AllowAnonymous();
        
        accountGroup.MapGet("/posts/recent",
            async (ClaimsPrincipal claimsPrincipal, AppDbContext dbContext, [FromQuery] int offset,
                [FromQuery] int limit) =>
            {
                var id = GetCurrentUserId(claimsPrincipal);
                var followingAccountIds = dbContext.Subscriptions
                    .Where(s => s.FollowerId == id) // currentUserId.Value, если AccountId - Value Object
                    .Select(s => s.FollowingId)
                    .AsQueryable(); // Важно, чтобы это был IQueryable, EF Core переведет Contains в IN (оптимизация)

                // 2. Находим посты, написанные этими аккаунтами
                var query = dbContext.PostDbSet
                    .Include(p => p.Author) // Eager load Author для UserName и PictureUrl
                    .Include(p => p.Comments).ThenInclude(p => p.Author)
                    .Where(p => followingAccountIds.Contains(p.Author.Id)); // Фильтруем по авторам
                
                // 4. Сортируем посты по дате создания (от новых к старым)
                query = query.OrderByDescending(p => p.CreatedAt);

                // 5. Ограничиваем количество постов
                query = query.Skip(offset).Take(limit);

                // 6. Проецируем результат в DTO
                var posts = await query
                    .Select(p => new
                    {
                        Id = p.Id,
                        Content = p.Content,
                        Images = p.Images,
                        AuthorId = p.Author.Id,
                        UserName = p.Author.UserName,
                        PictureUrl = p.Author.PictureUrl,
                        likedBy = p.LikedBy.Select(a => a.Id),
                        CreatedAt = p.CreatedAt,
                        Comments = p.Comments.OrderByDescending(c => c.CreatedAt).ToList(),
                        
                    })
                    .ToListAsync();

                return posts;
            });
        
        accountGroup.MapPost("unfollow/{targetAccountIdString}", async
             (string targetAccountIdString, AccountService subscriptionService, ClaimsPrincipal user) =>
            {
                var currentUserId = GetCurrentUserId(user);
                if (currentUserId == null)
                {
                    return Results.Unauthorized();
                }

                // 2. Парсим ID целевого аккаунта
                if (!Guid.TryParse(targetAccountIdString, out Guid targetGuid))
                {
                    return Results.BadRequest("Invalid target account ID format.");
                }

                AccountId targetAccountId = (AccountId)targetGuid;
                if (!await subscriptionService.UnfollowAsync(currentUserId.Value, targetAccountId))
                    return Results.BadRequest();
                // 3. Выполняем операцию отписки
                return Results.Ok();
            });
        
        accountGroup.MapPost("follow/{targetAccountIdString}",
            async (string targetAccountIdString, AccountService subscriptionService, ClaimsPrincipal user) =>
            {
                // 1. Получаем ID текущего пользователя
                var currentUserId = GetCurrentUserId(user);
                if (currentUserId == null)
                {
                    return Results.Unauthorized();
                }

                if (!Guid.TryParse(targetAccountIdString, out Guid targetGuid))
                {
                    return Results.BadRequest("Invalid target account ID format."); // 400 Bad Request
                }

                AccountId targetAccountId = (AccountId)targetGuid; // Преобразуем в AccountId

                // 3. Выполняем операцию подписки
                if (!await subscriptionService.FollowAsync(currentUserId.Value, targetAccountId))
                    return Results.BadRequest();

                return Results.Ok();

            });
        
        accountGroup.MapPut("/updateProfile", async (IFormFile file/*,IAntiforgery antiforgery*/, HttpContext context, AccountService service) =>
        {
            //await antiforgery.ValidateRequestAsync(context);
            var findFirst = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (findFirst is null)
            {
                return Results.Unauthorized();
            }

            await service.UpdateAvatarAsync(new AccountId(Guid.Parse(findFirst)), file);
            return Results.Ok();
        }).Accepts<IFormFile>("multipart/form-data").DisableAntiforgery();

        accountGroup.MapGet("/me", async (HttpContext context, AppDbContext dbContext, ILoggerFactory factory) =>
        {
            var value = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
    private static AccountId? GetCurrentUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userIdGuid))
        {
            return null;
        }
        return new AccountId(userIdGuid);
    }
}