namespace Thrume.Services;

public interface IUserPresenceTracker
{
    /// <summary>
    /// Tracks when a user connects to SignalR
    /// </summary>
    Task UserConnected(string userId, string connectionId);

    /// <summary>
    /// Tracks when a user disconnects from SignalR
    /// </summary>
    /// <returns>True if user is still online (has other connections), false if completely offline</returns>
    Task<bool> UserDisconnected(string userId, string connectionId);

    /// <summary>
    /// Tracks when a user joins a conversation
    /// </summary>
    Task UserJoinedConversation(string userId, string conversationId, string connectionId);

    /// <summary>
    /// Tracks when a user leaves a conversation
    /// </summary>
    Task UserLeftConversation(string userId, string conversationId, string connectionId);

    /// <summary>
    /// Checks if a user is currently online
    /// </summary>
    Task<bool> IsUserOnline(string userId);

    /// <summary>
    /// Checks if a user is currently in a specific conversation
    /// </summary>
    Task<bool> IsUserInConversation(string userId, string conversationId);

    /// <summary>
    /// Gets all online users in a conversation
    /// </summary>
    Task<List<string>> GetOnlineUsersInConversation(string conversationId);

    /// <summary>
    /// Gets all conversations a user is currently active in
    /// </summary>
    Task<List<string>> GetUserActiveConversations(string userId);

    /// <summary>
    /// Gets connection count for a user (for rate limiting)
    /// </summary>
    Task<int> GetUserConnectionCount(string userId);

    /// <summary>
    /// Gets all connection IDs for a user
    /// </summary>
    Task<List<string>> GetUserConnectionsAsync(string userId);
}