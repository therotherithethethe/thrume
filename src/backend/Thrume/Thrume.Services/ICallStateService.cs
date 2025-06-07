using Thrume.Domain.EntityIds;
using Thrume.Services.Models;

namespace Thrume.Services;

public interface ICallStateService
{
    Task<Call> CreateCallAsync(AccountId callerId, AccountId calleeId, CallType callType, string callerConnectionId);
    Task<Call?> GetActiveCallAsync(AccountId userId);
    Task<Call?> GetCallAsync(string callId);
    Task<bool> UpdateCallStatusAsync(string callId, CallStatus status);
    Task<bool> UpdateCallConnectionAsync(string callId, AccountId userId, string connectionId);
    Task<bool> EndCallAsync(string callId);
    Task<bool> IsUserAvailableForCallAsync(AccountId userId);
    Task<bool> CanUserCallAsync(AccountId callerId, AccountId calleeId);
    Task<Call[]> GetUserCallHistoryAsync(AccountId userId);
    Task CleanupExpiredCallsAsync();
}