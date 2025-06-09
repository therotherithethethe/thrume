using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Thrume.Domain.EntityIds;
using Thrume.Services;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Thrume.Database;
using Microsoft.AspNetCore.SignalR;
using Thrume.Api.Hubs;

namespace Thrume.Api.Endpoints;


internal record StartConversationPayload(AccountId OtherUserId);


internal record SendMessagePayload(ConversationId Id, string Content);


internal record ConversationListItemResponse(ConversationId Id, List<AccountId> ParticipantIds, DateTimeOffset CreatedAt);


internal record MessageResponse(MessageId Id, ConversationId ConversationId, AccountId SenderId, string Content, DateTimeOffset SentAt);

internal record PostMessageInConservation(ConversationId ConversationId, int Page, int PageSize);

public static class MessageEndpoints
{
    public static IEndpointRouteBuilder MapMessageEndpoints(this IEndpointRouteBuilder app)
    {
        var messagesGroup = app.MapGroup("/api/messages").RequireAuthorization();

        messagesGroup.MapGet("/conversations", async (HttpContext context, MessageService messageService) =>
        {
            var userId = GetCurrentUserId(context);
            if (userId == null) return Results.Unauthorized();

            var conversations = await messageService.GetConversationsAsync(userId.Value);
            
            return Results.Ok(conversations.Select(c => new
            {
                c.Id,
                c.CreatedAt,
                Participants = c.Participants.Select(p => new
                {
                    p.Id,
                    p.UserName,
                    p.PictureUrl
                })
            }));
        });


        messagesGroup.MapPost("/conversations/start/{userName}", async (
            HttpContext context,
            MessageService messageService/*, IAntiforgery antiforgery*/
            ,string userName) =>
        {
            //await antiforgery.ValidateRequestAsync(context);
            var userId = GetCurrentUserId(context);
            if (userId == null) return Results.Unauthorized();
            
            var conservationCanStart = await messageService.StartOrGetConversationAsync(userId.Value, userName);

            if (!conservationCanStart)
            {
                return Results.BadRequest("Unable to start conversation.");
            }
            
            return Results.Ok();
        }).RequireAuthorization("StandardAccess");

        messagesGroup.MapDelete("/conversations/{conservationId}", 
            async (HttpContext context, string conservationId, AppDbContext dbContext) =>
            {
                var id = GetCurrentUserId(context);
                if(!Guid.TryParse(conservationId, out var conversationId))
                    return Results.BadRequest();
                
                var conversation = await dbContext
                    .ConversationDbSet
                    .FirstOrDefaultAsync(c => c.Id == new ConversationId(conversationId) && c.Participants.Any(a => a.Id == id));
                if (conversation == null) return Results.BadRequest();

                dbContext.ConversationDbSet.Remove(conversation);
                await dbContext.SaveChangesAsync();
                return Results.Ok();
            }).RequireAuthorization("StandardAccess");


        messagesGroup.MapGet("/conversations/{conservationId}", async (
            HttpContext context,
            //[FromBody]PostMessageInConservation request,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            MessageService messageService, string conservationId) =>
        {
            var userId = GetCurrentUserId(context);
            if (userId == null) return Results.Unauthorized();


            var messages =
                await messageService.GetMessagesAsync(Guid.Parse(conservationId), userId.Value, page, pageSize);


            var response = messages.Select(m =>
                new MessageResponse(m.Id, m.ConversationId, m.SenderId, m.Content, m.SentAt)
            ).ToList();

            return Results.Ok(messages.Select(m => new
            {
                m.Id,
                m.ConversationId,
                m.SenderId,
                m.Sender.PictureUrl,
                m.Sender.UserName,
                m.Content,
                m.SentAt
            }));
        });


        messagesGroup.MapPost("/conversations/", async (
            HttpContext context,
            [FromBody] SendMessagePayload payload,
            MessageService messageService,
            IHubContext<ChatHub> hubContext,
            AppDbContext dbContext/*, IAntiforgery antiforgery*/) =>
        {
            //await antiforgery.ValidateRequestAsync(context);
            var userId = GetCurrentUserId(context);
            if (userId == null) return Results.Unauthorized();

            var message = await messageService.SendMessageAsync(userId.Value, payload.Id, payload.Content);

            if (message == null)
            {
                return Results.BadRequest("Unable to send message.");
            }

            try
            {
                // Get sender information for SignalR broadcast
                var sender = await dbContext.AccountDbSet
                    .Where(a => a.Id == userId.Value)
                    .Select(a => new { a.Id, a.UserName, a.PictureUrl })
                    .FirstOrDefaultAsync();

                var logger = context.RequestServices.GetRequiredService<ILogger<object>>();
                logger.LogInformation("Attempting SignalR broadcast for message {MessageId} to group conversation_{ConversationId}",
                    message.Id, payload.Id.Value);

                // Broadcast message to conversation group via SignalR
                var signalRMessage = new
                {
                    message.Id,
                    message.ConversationId,
                    message.SenderId,
                    SenderUserName = sender?.UserName,
                    SenderPictureUrl = sender?.PictureUrl,
                    message.Content,
                    message.SentAt
                };

                await hubContext.Clients.Group($"conversation_{payload.Id.Value}")
                    .SendAsync("ReceiveMessage", signalRMessage);
                    
                logger.LogInformation("Successfully broadcasted message {MessageId} via SignalR to group conversation_{ConversationId}",
                    message.Id, payload.Id.Value);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the message send
                // The message was saved successfully, SignalR broadcast is best-effort
                var logger = context.RequestServices.GetRequiredService<ILogger<object>>();
                logger.LogError(ex, "Failed to broadcast message {MessageId} via SignalR", message.Id);
            }

            var response = new MessageResponse(message.Id, message.ConversationId, message.SenderId, message.Content, message.SentAt);
            return Results.Ok(response);
        }).RequireAuthorization("StandardAccess");


        return app;
    }


    private static AccountId? GetCurrentUserId(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userIdGuid))
        {
            return new AccountId(userIdGuid);
        }
        return null;
    }
}