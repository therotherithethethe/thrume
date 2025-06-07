using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Thrume.Services;

public class UserPresenceTracker : IUserPresenceTracker
{
    private readonly ILogger<UserPresenceTracker> _logger;
    
    // User ID -> Set of Connection IDs
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, DateTime>> _userConnections = new();
    
    // Conversation ID -> User ID -> Set of Connection IDs
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, DateTime>>> _conversationUsers = new();
    
    // Connection ID -> User ID (for cleanup)
    private readonly ConcurrentDictionary<string, string> _connectionToUser = new();
    
    // Connection ID -> Set of Conversation IDs (for cleanup)
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, DateTime>> _connectionToConversations = new();

    private const int MaxConnectionsPerUser = 5;

    public UserPresenceTracker(ILogger<UserPresenceTracker> logger)
    {
        _logger = logger;
    }

    public Task UserConnected(string userId, string connectionId)
    {
        try
        {
            // Check connection limits
            var userConnections = _userConnections.GetOrAdd(userId, _ => new ConcurrentDictionary<string, DateTime>());
            
            if (userConnections.Count >= MaxConnectionsPerUser)
            {
                _logger.LogWarning("User {UserId} exceeded maximum connections limit ({MaxConnections})", userId, MaxConnectionsPerUser);
                // Remove oldest connection
                var oldestConnection = userConnections.OrderBy(kvp => kvp.Value).FirstOrDefault();
                if (!oldestConnection.Equals(default(KeyValuePair<string, DateTime>)))
                {
                    userConnections.TryRemove(oldestConnection.Key, out _);
                    _connectionToUser.TryRemove(oldestConnection.Key, out _);
                    _connectionToConversations.TryRemove(oldestConnection.Key, out _);
                }
            }

            userConnections.TryAdd(connectionId, DateTime.UtcNow);
            _connectionToUser.TryAdd(connectionId, userId);
            _connectionToConversations.TryAdd(connectionId, new ConcurrentDictionary<string, DateTime>());

            _logger.LogDebug("User {UserId} connected with connection {ConnectionId}. Total connections: {Count}",
                userId, connectionId, userConnections.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking user connection for {UserId} with connection {ConnectionId}", userId, connectionId);
        }

        return Task.CompletedTask;
    }

    public Task<bool> UserDisconnected(string userId, string connectionId)
    {
        try
        {
            // Remove connection from user's connection list
            if (_userConnections.TryGetValue(userId, out var userConnections))
            {
                userConnections.TryRemove(connectionId, out _);
                
                // If no connections left, remove user entirely
                if (userConnections.IsEmpty)
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }

            // Clean up connection mappings
            _connectionToUser.TryRemove(connectionId, out _);
            
            // Remove from all conversations this connection was in
            if (_connectionToConversations.TryRemove(connectionId, out var conversations))
            {
                foreach (var conversationId in conversations.Keys)
                {
                    if (_conversationUsers.TryGetValue(conversationId, out var conversationData) &&
                        conversationData.TryGetValue(userId, out var userConversationConnections))
                    {
                        userConversationConnections.TryRemove(connectionId, out _);
                        
                        // If user has no connections in this conversation, remove them
                        if (userConversationConnections.IsEmpty)
                        {
                            conversationData.TryRemove(userId, out _);
                            
                            // If conversation has no users, remove it
                            if (conversationData.IsEmpty)
                            {
                                _conversationUsers.TryRemove(conversationId, out _);
                            }
                        }
                    }
                }
            }

            bool isStillOnline = _userConnections.ContainsKey(userId) && !_userConnections[userId].IsEmpty;

            _logger.LogDebug("User {UserId} disconnected with connection {ConnectionId}. Still online: {IsOnline}",
                userId, connectionId, isStillOnline);

            return Task.FromResult(isStillOnline);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking user disconnection for {UserId} with connection {ConnectionId}", userId, connectionId);
            return Task.FromResult(false);
        }
    }

    public Task UserJoinedConversation(string userId, string conversationId, string connectionId)
    {
        try
        {
            var conversationData = _conversationUsers.GetOrAdd(conversationId, _ => new ConcurrentDictionary<string, ConcurrentDictionary<string, DateTime>>());
            var userConnections = conversationData.GetOrAdd(userId, _ => new ConcurrentDictionary<string, DateTime>());
            
            userConnections.TryAdd(connectionId, DateTime.UtcNow);

            // Track which conversations this connection is in
            if (_connectionToConversations.TryGetValue(connectionId, out var connectionConversations))
            {
                connectionConversations.TryAdd(conversationId, DateTime.UtcNow);
            }

            _logger.LogDebug("User {UserId} joined conversation {ConversationId} with connection {ConnectionId}",
                userId, conversationId, connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking user joining conversation {ConversationId} for {UserId} with connection {ConnectionId}",
                conversationId, userId, connectionId);
        }

        return Task.CompletedTask;
    }

    public Task UserLeftConversation(string userId, string conversationId, string connectionId)
    {
        try
        {
            if (_conversationUsers.TryGetValue(conversationId, out var conversationData) &&
                conversationData.TryGetValue(userId, out var userConnections))
            {
                userConnections.TryRemove(connectionId, out _);
                
                // If user has no connections in this conversation, remove them
                if (userConnections.IsEmpty)
                {
                    conversationData.TryRemove(userId, out _);
                    
                    // If conversation has no users, remove it
                    if (conversationData.IsEmpty)
                    {
                        _conversationUsers.TryRemove(conversationId, out _);
                    }
                }
            }

            // Remove conversation from connection tracking
            if (_connectionToConversations.TryGetValue(connectionId, out var connectionConversations))
            {
                connectionConversations.TryRemove(conversationId, out _);
            }

            _logger.LogDebug("User {UserId} left conversation {ConversationId} with connection {ConnectionId}",
                userId, conversationId, connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking user leaving conversation {ConversationId} for {UserId} with connection {ConnectionId}",
                conversationId, userId, connectionId);
        }

        return Task.CompletedTask;
    }

    public Task<bool> IsUserOnline(string userId)
    {
        var isOnline = _userConnections.ContainsKey(userId) && !_userConnections[userId].IsEmpty;
        return Task.FromResult(isOnline);
    }

    public Task<bool> IsUserInConversation(string userId, string conversationId)
    {
        var isInConversation = _conversationUsers.TryGetValue(conversationId, out var conversationData) &&
                              conversationData.ContainsKey(userId) &&
                              !conversationData[userId].IsEmpty;
        return Task.FromResult(isInConversation);
    }

    public Task<List<string>> GetOnlineUsersInConversation(string conversationId)
    {
        var users = new List<string>();
        
        if (_conversationUsers.TryGetValue(conversationId, out var conversationData))
        {
            users.AddRange(conversationData.Keys);
        }

        return Task.FromResult(users);
    }

    public Task<List<string>> GetUserActiveConversations(string userId)
    {
        var conversations = new List<string>();

        foreach (var kvp in _conversationUsers)
        {
            if (kvp.Value.ContainsKey(userId) && !kvp.Value[userId].IsEmpty)
            {
                conversations.Add(kvp.Key);
            }
        }

        return Task.FromResult(conversations);
    }

    public Task<int> GetUserConnectionCount(string userId)
    {
        var count = _userConnections.TryGetValue(userId, out var connections) ? connections.Count : 0;
        return Task.FromResult(count);
    }

    public Task<List<string>> GetUserConnectionsAsync(string userId)
    {
        var connectionIds = new List<string>();
        
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            connectionIds.AddRange(connections.Keys);
        }

        return Task.FromResult(connectionIds);
    }
}