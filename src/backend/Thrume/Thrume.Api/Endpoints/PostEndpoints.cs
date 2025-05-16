
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Thrume.Database;
using Thrume.Domain.DTOs;
using Thrume.Domain.EntityIds;
using Thrume.Services;

namespace Thrume.Api.Endpoints;

public static class PostEndpoints
{
    public static IEndpointRouteBuilder MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var postsGroup = app.MapGroup("/posts").RequireAuthorization(); 


        postsGroup.MapPost("/", async (
            HttpContext context,
            AppDbContext dbContext,
            [FromForm] CreatePostRequest createPostRequest, AccountService service, IAntiforgery antiforgery) =>
        {
            await antiforgery.ValidateRequestAsync(context);
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)!;
            var guid = Guid.Parse(userIdClaim.Value);
            await service.CreatePostsAsync(new AccountId(guid), createPostRequest);
            return Results.Ok();
        }).Accepts<IFormFile>("multipart/form-data").RequireAuthorization();


        postsGroup.MapGet("/{userName}", async ([FromRoute]string userName, HttpContext context, AppDbContext dbContext) =>
        {

            var acc = await dbContext.PostDbSet
                .AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.Author.UserName == userName)
                .Select(p => new {p.Id, p.Content, p.Images, LikedBy = p.LikedBy.Select(acc => acc.Id), p.CreatedAt})
                .ToListAsync();

            return Results.Ok(acc);
        }).AllowAnonymous();

        postsGroup.MapPut("/like", async (HttpContext context, [FromBody] PostId id, PostService postService, IAntiforgery antiforgery) =>
        {
            await antiforgery.ValidateRequestAsync(context);
            var accIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var accId = new AccountId(Guid.Parse(accIdStr));
            await postService.LikePostAsync(accId, id);

            return Results.Ok(); 
        });

        return app; 
    }
}