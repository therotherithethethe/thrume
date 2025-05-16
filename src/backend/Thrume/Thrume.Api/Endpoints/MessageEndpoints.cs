using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Thrume.Domain.EntityIds;
using Thrume.Services; 

using Microsoft.AspNetCore.Antiforgery;

namespace Thrume.Api.Endpoints;


internal record StartConversationPayload(AccountId OtherUserId);


internal record SendMessagePayload(ConversationId Id, string Content);


internal record ConversationListItemResponse(ConversationId Id, List<AccountId> ParticipantIds, DateTimeOffset CreatedAt);


internal record MessageResponse(MessageId Id, ConversationId ConversationId, AccountId SenderId, string Content, DateTimeOffset SentAt);


public static class MessageEndpoints
{
    public static IEndpointRouteBuilder MapMessageEndpoints(this IEndpointRouteBuilder app)
    {
        var messagesGroup = app.MapGroup("/messages").RequireAuthorization();

        messagesGroup.MapGet("/conversations", async (HttpContext context, MessageService messageService) =>
        {
            var userId = GetCurrentUserId(context);
            if (userId == null) return Results.Unauthorized();

            var conversations = await messageService.GetConversationsAsync(userId.Value);


            var response = conversations.Select(c =>
                new ConversationListItemResponse(
                    c.Id,
                    c.Participants.Select(p => p.Id).ToList(), 
                    c.CreatedAt)
                ).ToList();

            return Results.Ok(response);
        });


        messagesGroup.MapPost("/conversations/start", async (
            HttpContext context,
            [FromBody] StartConversationPayload payload,
            MessageService messageService, IAntiforgery antiforgery) =>
        {
            await antiforgery.ValidateRequestAsync(context);
            var userId = GetCurrentUserId(context);
            if (userId == null) return Results.Unauthorized();

            var conversation = await messageService.StartOrGetConversationAsync(userId.Value, payload.OtherUserId);

            if (conversation == null)
            {

                return Results.BadRequest("Unable to start conversation.");
            }


            var response = new ConversationListItemResponse(
                conversation.Id,
                conversation.Participants.Select(p => p.Id).ToList(),
                conversation.CreatedAt);

            return Results.Ok(response);
        });


        messagesGroup.MapGet("/conversations/{conversationId}", async (
            HttpContext context,
            [FromBody] ConversationId conversationId,
            [FromQuery] int page, 
            [FromQuery] int pageSize,
            MessageService messageService) =>
        {
             var userId = GetCurrentUserId(context);
            if (userId == null) return Results.Unauthorized();


            var messages = await messageService.GetMessagesAsync(conversationId, userId.Value, page, pageSize);


            var response = messages.Select(m =>
                new MessageResponse(m.Id, m.ConversationId, m.SenderId, m.Content, m.SentAt)
                ).ToList();

            return Results.Ok(response);
        });


        messagesGroup.MapPost("/conversations/", async (
            HttpContext context,
            [FromBody] SendMessagePayload payload,
            MessageService messageService, IAntiforgery antiforgery) =>
        {
            await antiforgery.ValidateRequestAsync(context);
            var userId = GetCurrentUserId(context);
            if (userId == null) return Results.Unauthorized();

            var message = await messageService.SendMessageAsync(userId.Value, payload.Id, payload.Content);

            if (message == null)
            {

                return Results.BadRequest("Unable to send message.");
            }


            var response = new MessageResponse(message.Id, message.ConversationId, message.SenderId, message.Content, message.SentAt);
            return Results.Ok();
        });


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