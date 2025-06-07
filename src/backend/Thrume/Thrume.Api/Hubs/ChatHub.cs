using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Thrume.Domain.EntityIds;
using Thrume.Services;
using Thrume.Services.Models;
using Microsoft.Extensions.Logging;

namespace Thrume.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IUserPresenceTracker _presenceTracker;
    private readonly MessageService _messageService;
    private readonly ICallStateService _callStateService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IUserPresenceTracker presenceTracker, MessageService messageService, ICallStateService callStateService, ILogger<ChatHub> logger)
    {
        _presenceTracker = presenceTracker;
        _messageService = messageService;
        _callStateService = callStateService;
        _logger = logger;
    }

    public async Task JoinConversationAsync(string conversationId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized user attempted to join conversation {ConversationId}", conversationId);
                return;
            }

            // Validate that the user is a participant in the conversation
            var conversations = await _messageService.GetConversationsAsync(userId.Value);
            var targetConversation = conversations.FirstOrDefault(c => c.Id.Value.ToString() == conversationId);
            
            if (targetConversation == null)
            {
                _logger.LogWarning("User {UserId} attempted to join unauthorized conversation {ConversationId}", userId.Value, conversationId);
                await Clients.Caller.SendAsync("Error", "You are not authorized to join this conversation");
                return;
            }

            // Add user to conversation group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            _logger.LogInformation("Added connection {ConnectionId} to group conversation_{ConversationId}", Context.ConnectionId, conversationId);
            
            // Track user presence in conversation
            await _presenceTracker.UserJoinedConversation(userId.Value.ToString(), conversationId, Context.ConnectionId);
            
            // Notify other users in the conversation
            await Clients.Group($"conversation_{conversationId}")
                .SendAsync("UserJoined", conversationId, userId.Value.ToString());

            _logger.LogInformation("User {UserId} with connection {ConnectionId} joined conversation {ConversationId}", userId.Value, Context.ConnectionId, conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining conversation {ConversationId}", conversationId);
            await Clients.Caller.SendAsync("Error", "Failed to join conversation");
        }
    }

    public async Task LeaveConversationAsync(string conversationId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return;
            }

            // Remove user from conversation group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            
            // Update presence tracking
            await _presenceTracker.UserLeftConversation(userId.Value.ToString(), conversationId, Context.ConnectionId);
            
            // Notify other users in the conversation
            await Clients.Group($"conversation_{conversationId}")
                .SendAsync("UserLeft", conversationId, userId.Value.ToString());

            _logger.LogInformation("User {UserId} left conversation {ConversationId}", userId.Value, conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving conversation {ConversationId}", conversationId);
        }
    }

    public async Task SendTypingIndicatorAsync(string conversationId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return;
            }

            // Verify user is in conversation
            if (!await _presenceTracker.IsUserInConversation(userId.Value.ToString(), conversationId))
            {
                return;
            }

            // Broadcast typing indicator to other users in conversation (excluding sender)
            await Clients.GroupExcept($"conversation_{conversationId}", Context.ConnectionId)
                .SendAsync("TypingIndicator", conversationId, userId.Value.ToString(), true);

            _logger.LogDebug("User {UserId} started typing in conversation {ConversationId}", userId.Value, conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending typing indicator for conversation {ConversationId}", conversationId);
        }
    }

    public async Task StopTypingIndicatorAsync(string conversationId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return;
            }

            // Verify user is in conversation
            if (!await _presenceTracker.IsUserInConversation(userId.Value.ToString(), conversationId))
            {
                return;
            }

            // Broadcast stop typing indicator to other users in conversation (excluding sender)
            await Clients.GroupExcept($"conversation_{conversationId}", Context.ConnectionId)
                .SendAsync("TypingIndicator", conversationId, userId.Value.ToString(), false);

            _logger.LogDebug("User {UserId} stopped typing in conversation {ConversationId}", userId.Value, conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping typing indicator for conversation {ConversationId}", conversationId);
        }
    }

    // ===============================
    // WebRTC Calling Methods
    // ===============================

    public async Task InitiateCallAsync(string calleeId, string callType, string conversationId = "")
    {
        try
        {
            _logger.LogInformation("🔵 [ChatHub] Received call initiation request: CalleeId={CalleeId}, CallType={CallType}, ConversationId={ConversationId}", calleeId, callType, conversationId);
            
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                _logger.LogWarning("❌ [ChatHub] Unauthorized user attempted to initiate call to {CalleeId}", calleeId);
                await Clients.Caller.SendAsync("CallError", "Unauthorized");
                return;
            }
            _logger.LogInformation("✅ [ChatHub] User authenticated: {UserId}", userId.Value);

            if (!Guid.TryParse(calleeId, out var calleeGuid))
            {
                _logger.LogWarning("❌ [ChatHub] Invalid callee ID format: {CalleeId}", calleeId);
                await Clients.Caller.SendAsync("CallError", "Invalid callee ID");
                return;
            }

            var calleeAccountId = new AccountId(calleeGuid);
            if (!Enum.TryParse<CallType>(callType, true, out var callTypeEnum))
            {
                _logger.LogWarning("❌ [ChatHub] Invalid call type: {CallType}", callType);
                await Clients.Caller.SendAsync("CallError", "Invalid call type");
                return;
            }
            _logger.LogInformation("✅ [ChatHub] Call parameters valid: CalleeId={CalleeId}, CallType={CallType}", calleeAccountId.Value, callTypeEnum);

            // Check if users can call each other
            _logger.LogInformation("🔍 [ChatHub] Checking if user can call...");
            if (!await _callStateService.CanUserCallAsync(userId.Value, calleeAccountId))
            {
                _logger.LogWarning("❌ [ChatHub] User {UserId} cannot call {CalleeId} - not available", userId.Value, calleeId);
                await Clients.Caller.SendAsync("CallError", "User is not available for calls");
                return;
            }
            _logger.LogInformation("✅ [ChatHub] User can make call");

            // Create the call
            _logger.LogInformation("📞 [ChatHub] Creating call...");
            var call = await _callStateService.CreateCallAsync(userId.Value, calleeAccountId, callTypeEnum, Context.ConnectionId);
            _logger.LogInformation("✅ [ChatHub] Call created: {CallId}", call.Id);

            // Get callee's connection ID from presence tracker
            _logger.LogInformation("🔍 [ChatHub] Getting callee connection IDs...");
            var calleeConnectionIds = await _presenceTracker.GetUserConnectionsAsync(calleeId);
            _logger.LogInformation("📡 [ChatHub] Found {Count} connection(s) for callee: {Connections}", calleeConnectionIds.Count(), string.Join(",", calleeConnectionIds));
            
            if (!calleeConnectionIds.Any())
            {
                _logger.LogWarning("❌ [ChatHub] Callee {CalleeId} is not online - no connections found", calleeId);
                await _callStateService.UpdateCallStatusAsync(call.Id, CallStatus.Failed);
                await Clients.Caller.SendAsync("CallError", "User is not online");
                return;
            }

            // Update call status to ringing
            _logger.LogInformation("📞 [ChatHub] Updating call status to Ringing...");
            await _callStateService.UpdateCallStatusAsync(call.Id, CallStatus.Ringing);

            // Create the IncomingCall event payload
            var incomingCallPayload = new
            {
                callId = call.Id,
                callerId = userId.Value.ToString(),
                callerUsername = Context.User?.Identity?.Name ?? "Unknown",
                callType = callType,
                conversationId = conversationId, // FIXED: Now using the passed conversationId
                timestamp = DateTime.UtcNow.ToString("O"),
                offer = (object?)null // Placeholder - actual offer will be sent later via ReceiveOffer
            };
            
            _logger.LogInformation("📡 [ChatHub] Sending IncomingCall to {Count} connections: {Payload}", calleeConnectionIds.Count(), System.Text.Json.JsonSerializer.Serialize(incomingCallPayload));

            // Notify the callee about incoming call
            await Clients.Clients(calleeConnectionIds).SendAsync("IncomingCall", incomingCallPayload);

            // Notify caller that call is ringing
            _logger.LogInformation("📡 [ChatHub] Sending CallRinging to caller...");
            await Clients.Caller.SendAsync("CallRinging", call.Id);

            _logger.LogInformation("✅ [ChatHub] Call {CallId} initiated successfully from {CallerId} to {CalleeId}", call.Id, userId.Value, calleeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ChatHub] Error initiating call to {CalleeId}", calleeId);
            await Clients.Caller.SendAsync("CallError", "Failed to initiate call");
        }
    }

    public async Task AcceptCallAsync(string callId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("CallError", "Unauthorized");
                return;
            }

            var call = await _callStateService.GetCallAsync(callId);
            if (call == null)
            {
                await Clients.Caller.SendAsync("CallError", "Call not found");
                return;
            }

            // Only the callee can accept the call
            if (call.CalleeId != userId.Value)
            {
                await Clients.Caller.SendAsync("CallError", "Not authorized to accept this call");
                return;
            }

            // Update call connection info
            await _callStateService.UpdateCallConnectionAsync(callId, userId.Value, Context.ConnectionId);
            await _callStateService.UpdateCallStatusAsync(callId, CallStatus.Connected);

            // Notify both parties
            if (!string.IsNullOrEmpty(call.CallerConnectionId))
            {
                await Clients.Client(call.CallerConnectionId).SendAsync("CallAccepted", callId);
            }
            await Clients.Caller.SendAsync("CallConnected", callId);

            _logger.LogInformation("Call {CallId} accepted by {UserId}", callId, userId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting call {CallId}", callId);
            await Clients.Caller.SendAsync("CallError", "Failed to accept call");
        }
    }

    public async Task RejectCallAsync(string callId, string reason = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("CallError", "Unauthorized");
                return;
            }

            var call = await _callStateService.GetCallAsync(callId);
            if (call == null)
            {
                await Clients.Caller.SendAsync("CallError", "Call not found");
                return;
            }

            // Only the callee can reject the call
            if (call.CalleeId != userId.Value)
            {
                await Clients.Caller.SendAsync("CallError", "Not authorized to reject this call");
                return;
            }

            await _callStateService.UpdateCallStatusAsync(callId, CallStatus.Rejected);

            // Notify the caller
            if (!string.IsNullOrEmpty(call.CallerConnectionId))
            {
                await Clients.Client(call.CallerConnectionId).SendAsync("CallRejected", callId, reason);
            }

            _logger.LogInformation("Call {CallId} rejected by {UserId}", callId, userId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting call {CallId}", callId);
            await Clients.Caller.SendAsync("CallError", "Failed to reject call");
        }
    }

    public async Task EndCallAsync(string callId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("CallError", "Unauthorized");
                return;
            }

            var call = await _callStateService.GetCallAsync(callId);
            if (call == null)
            {
                await Clients.Caller.SendAsync("CallError", "Call not found");
                return;
            }

            // Either participant can end the call
            if (call.CallerId != userId.Value && call.CalleeId != userId.Value)
            {
                await Clients.Caller.SendAsync("CallError", "Not authorized to end this call");
                return;
            }

            await _callStateService.EndCallAsync(callId);

            // Notify both parties
            var notifyConnections = new List<string>();
            if (!string.IsNullOrEmpty(call.CallerConnectionId))
                notifyConnections.Add(call.CallerConnectionId);
            if (!string.IsNullOrEmpty(call.CalleeConnectionId))
                notifyConnections.Add(call.CalleeConnectionId);

            await Clients.Clients(notifyConnections).SendAsync("CallEnded", callId);

            _logger.LogInformation("Call {CallId} ended by {UserId}", callId, userId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending call {CallId}", callId);
            await Clients.Caller.SendAsync("CallError", "Failed to end call");
        }
    }

    // WebRTC Signaling Methods
    public async Task SendOfferAsync(string callId, string sdpOffer)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return;

            var call = await _callStateService.GetCallAsync(callId);
            if (call == null || call.CallerId != userId.Value) return;

            if (!string.IsNullOrEmpty(call.CalleeConnectionId))
            {
                await Clients.Client(call.CalleeConnectionId).SendAsync("ReceiveOffer", callId, sdpOffer);
            }

            _logger.LogDebug("SDP offer sent for call {CallId}", callId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending offer for call {CallId}", callId);
        }
    }

    public async Task SendAnswerAsync(string callId, string sdpAnswer)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return;

            var call = await _callStateService.GetCallAsync(callId);
            if (call == null || call.CalleeId != userId.Value) return;

            if (!string.IsNullOrEmpty(call.CallerConnectionId))
            {
                await Clients.Client(call.CallerConnectionId).SendAsync("ReceiveAnswer", callId, sdpAnswer);
            }

            _logger.LogDebug("SDP answer sent for call {CallId}", callId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending answer for call {CallId}", callId);
        }
    }

    public async Task SendIceCandidateAsync(string callId, string candidate)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return;

            var call = await _callStateService.GetCallAsync(callId);
            if (call == null) return;

            // Either participant can send ICE candidates
            if (call.CallerId != userId.Value && call.CalleeId != userId.Value) return;

            // Send to the other participant
            string targetConnectionId = null;
            if (call.CallerId == userId.Value && !string.IsNullOrEmpty(call.CalleeConnectionId))
            {
                targetConnectionId = call.CalleeConnectionId;
            }
            else if (call.CalleeId == userId.Value && !string.IsNullOrEmpty(call.CallerConnectionId))
            {
                targetConnectionId = call.CallerConnectionId;
            }

            if (!string.IsNullOrEmpty(targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", callId, candidate);
            }

            _logger.LogDebug("ICE candidate sent for call {CallId}", callId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending ICE candidate for call {CallId}", callId);
        }
    }

    public async Task UpdateCallStatusAsync(string callId, string status)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return;

            var call = await _callStateService.GetCallAsync(callId);
            if (call == null) return;

            if (call.CallerId != userId.Value && call.CalleeId != userId.Value) return;

            if (Enum.TryParse<CallStatus>(status, true, out var callStatus))
            {
                await _callStateService.UpdateCallStatusAsync(callId, callStatus);
                _logger.LogDebug("Call {CallId} status updated to {Status}", callId, status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating call status for call {CallId}", callId);
        }
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId != null)
            {
                await _presenceTracker.UserConnected(userId.Value.ToString(), Context.ConnectionId);
                
                // Notify contacts about user coming online
                await NotifyContactsPresenceChange(userId.Value.ToString(), true);
                
                _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId.Value, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling user connection");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId != null)
            {
                var isStillOnline = await _presenceTracker.UserDisconnected(userId.Value.ToString(), Context.ConnectionId);
                
                // Only notify if user is completely offline
                if (!isStillOnline)
                {
                    await NotifyContactsPresenceChange(userId.Value.ToString(), false);
                }
                
                _logger.LogInformation("User {UserId} disconnected with connection {ConnectionId}", userId.Value, Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling user disconnection");
        }
    }

    private AccountId? GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
        {
            return new AccountId(userId);
        }
        return null;
    }

    private async Task NotifyContactsPresenceChange(string userId, bool isOnline)
    {
        try
        {
            // Get user's conversations to notify participants
            var accountId = new AccountId(Guid.Parse(userId));
            var conversations = await _messageService.GetConversationsAsync(accountId);
            
            foreach (var conversation in conversations)
            {
                await Clients.Group($"conversation_{conversation.Id.Value}")
                    .SendAsync("PresenceUpdated", userId, isOnline);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying contacts about presence change for user {UserId}", userId);
        }
    }
}