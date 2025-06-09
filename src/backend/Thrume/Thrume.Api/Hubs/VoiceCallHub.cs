using Microsoft.AspNetCore.SignalR;

namespace Thrume.Api.Hubs;

public class VoiceCallHub : Hub
{
    // Relays the offer SDP to the target user
    public async Task SendOffer(string targetConnectionId, string sdpOffer)
    {
        // In a real app, you'd validate targetConnectionId and user permissions
        await Clients.Client(targetConnectionId).SendAsync("ReceiveOffer", Context.ConnectionId, sdpOffer);
    }

    // Relays the answer SDP back to the caller
    public async Task SendAnswer(string targetConnectionId, string sdpAnswer)
    {

        await Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer", Context.ConnectionId, sdpAnswer);
    }

    // Relays ICE candidates to the target user
    public async Task SendIceCandidate(string targetConnectionId, string iceCandidate)
    {
        // ICE candidates are typically sent as JSON strings
        await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", Context.ConnectionId, iceCandidate);
    }

    // Optional: Basic handling for user leaving (can be expanded)
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Example: Notify others if users were in rooms/groups
        // await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SomeRoom");
        // await Clients.Group("SomeRoom").SendAsync("UserLeft", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}