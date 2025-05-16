using System.Collections;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thrume.Domain.EntityIds;
using Thrume.Services;

namespace Thrume.Api.Endpoints;

internal record CreateCommentPayload(PostId PostId, string Content, CommentId? ParentCommentId);

public static class CommentEndpoints
{
    public static IEndpointRouteBuilder MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var commentsGroup = app.MapGroup("/comments").RequireAuthorization();
        
        commentsGroup.MapPost("/", async (
            HttpContext context,
            [FromBody] CreateCommentPayload payload,
            CommentService commentService, IAntiforgery antiforgery) =>
        {
            await antiforgery.ValidateRequestAsync(context);
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userIdGuid))
            {
                return Results.Unauthorized();
            }
            var authorId = new AccountId(userIdGuid);
            
            if (string.IsNullOrWhiteSpace(payload.Content) || payload.Content.Length > 1000)
            {
                return Results.BadRequest("Comment content is invalid or too long.");
            }
            await commentService.CreateCommentAsync(payload.PostId, authorId, payload.Content, payload.ParentCommentId);

            return Results.Ok(); 
        });
        
        commentsGroup.MapDelete("/", async (
            HttpContext context,
            [FromBody] CommentId commentId, 
            CommentService commentService) =>
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userIdGuid))
            {
                return Results.Unauthorized(); 
            }
            var userId = new AccountId(userIdGuid);


            await commentService.DeleteCommentAsync(commentId, userId);


            return Results.NoContent();
        });

        return app;
    }
}