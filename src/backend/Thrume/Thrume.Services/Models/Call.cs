using Thrume.Domain.EntityIds;

namespace Thrume.Services.Models;

public class Call
{
    public string Id { get; set; } = null!;
    public AccountId CallerId { get; set; }
    public AccountId CalleeId { get; set; }
    public CallType Type { get; set; }
    public CallStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? CallerConnectionId { get; set; }
    public string? CalleeConnectionId { get; set; }
}

public enum CallType
{
    Voice = 0,
    Video = 1
}

public enum CallStatus
{
    Initiated = 0,
    Ringing = 1,
    Connected = 2,
    Ended = 3,
    Rejected = 4,
    Missed = 5,
    Failed = 6
}