
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

        postsGroup.MapDelete("/", async (HttpContext context, [FromBody]PostId id, PostService service) =>
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)!;
            var guid = Guid.Parse(userIdClaim.Value);
            return await service.DeletePostAsync(id, guid) ? Results.Ok() : Results.BadRequest();
        });
        postsGroup.MapPost("/", async (
            HttpContext context,
            AppDbContext dbContext,
            [FromForm]CreatePostRequest createPostRequest, AccountService service/*, IAntiforgery antiforgery*/) =>
        {
            //await antiforgery.ValidateRequestAsync(context);
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)!;
            var guid = Guid.Parse(userIdClaim.Value);
            await service.CreatePostsAsync(new AccountId(guid), createPostRequest);
            return Results.Ok();
        }).Accepts<IFormFile>("multipart/form-data").DisableAntiforgery().RequireAuthorization();


        postsGroup.MapGet("/{userName}", async ([FromRoute]string userName, HttpContext context, AppDbContext dbContext) =>
        {
            
            var acc = await dbContext.PostDbSet
                .AsNoTracking()
                .Include(p => p.Images)
                .Include(p => p.Comments)
                .ThenInclude(p => p.Author)
                .Where(p => p.Author.UserName == userName)
                .Select(p => new
                {
                    p.Id, p.Content, 
                    p.Images, 
                    authorId = p.Author.Id ,
                    LikedBy = p.LikedBy.Select(acc => acc.Id), 
                    p.CreatedAt,
                    Comments = p.Comments.OrderByDescending(c => c.CreatedAt).ToList(),
                    p.Author.UserName,
                    p.Author.PictureUrl
                })
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return Results.Ok(acc);
        }).AllowAnonymous();

        postsGroup.MapPut("/like", async (HttpContext context, [FromBody] PostId id, PostService postService/*, IAntiforgery antiforgery*/) =>
        {
            //await antiforgery.ValidateRequestAsync(context);
            var accIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var accId = new AccountId(Guid.Parse(accIdStr));
            await postService.LikePostAsync(accId, id);

            return Results.Ok(); 
        });
        postsGroup.MapPut("/unlike", async (HttpContext context, [FromBody] PostId id, PostService postService/*, IAntiforgery antiforgery*/) =>
        {
            //await antiforgery.ValidateRequestAsync(context);
            var accIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var accId = new AccountId(Guid.Parse(accIdStr));
            await postService.UnlikePostAsync(accId, id);

            return Results.Ok(); 
        });

        return app; 
    }
}