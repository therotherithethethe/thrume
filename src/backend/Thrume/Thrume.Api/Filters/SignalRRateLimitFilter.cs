using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Thrume.Api.Filters;

public class SignalRRateLimitFilter : IHubFilter
{
    private readonly ILogger<SignalRRateLimitFilter> _logger;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _userRequestTimes = new();
    private const int MaxMessagesPerMinute = 60;
    private readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(1);

    public SignalRRateLimitFilter(ILogger<SignalRRateLimitFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext, 
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var userId = GetUserId(invocationContext.Context);
        if (userId == null)
        {
            _logger.LogWarning("Anonymous user attempted to invoke hub method {Method}", invocationContext.HubMethodName);
            throw new HubException("Authentication required");
        }

        // Check rate limit for specific methods
        if (ShouldRateLimit(invocationContext.HubMethodName))
        {
            if (!IsWithinRateLimit(userId))
            {
                _logger.LogWarning("User {UserId} exceeded rate limit for method {Method}", userId, invocationContext.HubMethodName);
                throw new HubException("Rate limit exceeded. Please slow down.");
            }
        }

        try
        {
            return await next(invocationContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing hub method {Method} for user {UserId}", 
                invocationContext.HubMethodName, userId);
            throw;
        }
    }

    private static string? GetUserId(HubCallerContext context)
    {
        return context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private static bool ShouldRateLimit(string methodName)
    {
        return methodName is "SendTypingIndicatorAsync" or "StopTypingIndicatorAsync";
    }

    private bool IsWithinRateLimit(string userId)
    {
        var now = DateTime.UtcNow;
        var userTimes = _userRequestTimes.GetOrAdd(userId, _ => new ConcurrentQueue<DateTime>());

        // Remove old requests outside the time window
        while (userTimes.TryPeek(out var oldest) && now - oldest > _timeWindow)
        {
            userTimes.TryDequeue(out _);
        }

        // Check if within limit
        if (userTimes.Count >= MaxMessagesPerMinute)
        {
            return false;
        }

        // Add current request
        userTimes.Enqueue(now);
        return true;
    }
}