using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Thrume.Domain.EntityIds;
using Thrume.Services.Models;

namespace Thrume.Services;

public class CallStateService : ICallStateService
{
    private readonly ConcurrentDictionary<string, Call> _activeCalls = new();
    private readonly ConcurrentDictionary<AccountId, string> _userActiveCalls = new(); // UserId -> CallId
    private readonly ConcurrentDictionary<AccountId, List<Call>> _callHistory = new();
    private readonly ILogger<CallStateService> _logger;

    public CallStateService(ILogger<CallStateService> logger)
    {
        _logger = logger;
    }

    public Task<Call> CreateCallAsync(AccountId callerId, AccountId calleeId, CallType callType, string callerConnectionId)
    {
        var callId = Guid.NewGuid().ToString();
        var call = new Call
        {
            Id = callId,
            CallerId = callerId,
            CalleeId = calleeId,
            Type = callType,
            Status = CallStatus.Initiated,
            StartedAt = DateTime.UtcNow,
            CallerConnectionId = callerConnectionId
        };

        _activeCalls[callId] = call;
        _userActiveCalls[callerId] = callId;
        _userActiveCalls[calleeId] = callId;

        // Add to call history
        AddToCallHistory(callerId, call);
        AddToCallHistory(calleeId, call);

        _logger.LogInformation("Created call {CallId} from {CallerId} to {CalleeId}", callId, callerId, calleeId);
        return Task.FromResult(call);
    }

    public Task<Call?> GetActiveCallAsync(AccountId userId)
    {
        if (_userActiveCalls.TryGetValue(userId, out var callId) && 
            _activeCalls.TryGetValue(callId, out var call))
        {
            return Task.FromResult<Call?>(call);
        }
        return Task.FromResult<Call?>(null);
    }

    public Task<Call?> GetCallAsync(string callId)
    {
        _activeCalls.TryGetValue(callId, out var call);
        return Task.FromResult(call);
    }

    public Task<bool> UpdateCallStatusAsync(string callId, CallStatus status)
    {
        if (!_activeCalls.TryGetValue(callId, out var call))
        {
            return Task.FromResult(false);
        }

        call.Status = status;

        switch (status)
        {
            case CallStatus.Connected:
                call.ConnectedAt = DateTime.UtcNow;
                break;
            case CallStatus.Ended:
            case CallStatus.Rejected:
            case CallStatus.Missed:
            case CallStatus.Failed:
                call.EndedAt = DateTime.UtcNow;
                // Remove from active calls
                _activeCalls.TryRemove(callId, out _);
                _userActiveCalls.TryRemove(call.CallerId, out _);
                _userActiveCalls.TryRemove(call.CalleeId, out _);
                break;
        }

        _logger.LogInformation("Updated call {CallId} status to {Status}", callId, status);
        return Task.FromResult(true);
    }

    public Task<bool> UpdateCallConnectionAsync(string callId, AccountId userId, string connectionId)
    {
        if (!_activeCalls.TryGetValue(callId, out var call))
        {
            return Task.FromResult(false);
        }

        if (call.CallerId == userId)
        {
            call.CallerConnectionId = connectionId;
        }
        else if (call.CalleeId == userId)
        {
            call.CalleeConnectionId = connectionId;
        }
        else
        {
            return Task.FromResult(false);
        }

        _logger.LogDebug("Updated connection for user {UserId} in call {CallId}", userId, callId);
        return Task.FromResult(true);
    }

    public Task<bool> EndCallAsync(string callId)
    {
        return UpdateCallStatusAsync(callId, CallStatus.Ended);
    }

    public Task<bool> IsUserAvailableForCallAsync(AccountId userId)
    {
        var hasActiveCall = _userActiveCalls.ContainsKey(userId);
        return Task.FromResult(!hasActiveCall);
    }

    public Task<bool> CanUserCallAsync(AccountId callerId, AccountId calleeId)
    {
        _logger.LogInformation("🔍 [CallStateService] Checking if call is allowed: CallerId={CallerId}, CalleeId={CalleeId}", callerId.Value, calleeId.Value);
        
        // Cannot call yourself
        if (callerId == calleeId)
        {
            _logger.LogWarning("❌ [CallStateService] Cannot call yourself: {UserId}", callerId.Value);
            return Task.FromResult(false);
        }

        // Check if either user has an active call
        var callerAvailable = !_userActiveCalls.ContainsKey(callerId);
        var calleeAvailable = !_userActiveCalls.ContainsKey(calleeId);
        
        _logger.LogInformation("🔍 [CallStateService] Availability check: CallerAvailable={CallerAvailable}, CalleeAvailable={CalleeAvailable}", callerAvailable, calleeAvailable);
        
        if (!callerAvailable && _userActiveCalls.TryGetValue(callerId, out var callerCallId))
        {
            _logger.LogWarning("❌ [CallStateService] Caller {CallerId} has active call: {CallId}", callerId.Value, callerCallId);
        }
        
        if (!calleeAvailable && _userActiveCalls.TryGetValue(calleeId, out var calleeCallId))
        {
            _logger.LogWarning("❌ [CallStateService] Callee {CalleeId} has active call: {CallId}", calleeId.Value, calleeCallId);
        }

        var result = callerAvailable && calleeAvailable;
        _logger.LogInformation("🔍 [CallStateService] Can call result: {CanCall}", result);
        
        return Task.FromResult(result);
    }

    public Task<Call[]> GetUserCallHistoryAsync(AccountId userId)
    {
        if (_callHistory.TryGetValue(userId, out var history))
        {
            return Task.FromResult(history.OrderByDescending(c => c.StartedAt).ToArray());
        }
        return Task.FromResult(Array.Empty<Call>());
    }

    public Task CleanupExpiredCallsAsync()
    {
        var expiredCalls = _activeCalls.Values
            .Where(c => c.StartedAt < DateTime.UtcNow.AddMinutes(-30)) // Cleanup calls older than 30 minutes
            .ToList();

        foreach (var call in expiredCalls)
        {
            _activeCalls.TryRemove(call.Id, out _);
            _userActiveCalls.TryRemove(call.CallerId, out _);
            _userActiveCalls.TryRemove(call.CalleeId, out _);
            _logger.LogInformation("Cleaned up expired call {CallId}", call.Id);
        }

        return Task.CompletedTask;
    }

    private void AddToCallHistory(AccountId userId, Call call)
    {
        _callHistory.AddOrUpdate(
            userId,
            [call],
            (key, existing) =>
            {
                existing.Add(call);
                // Keep only last 100 calls per user
                if (existing.Count > 100)
                {
                    existing.RemoveRange(0, existing.Count - 100);
                }
                return existing;
            });
    }
}