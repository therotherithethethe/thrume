using Thrume.Domain.EntityIds;

namespace Thrume.Domain.Entity;

public sealed class Subscription
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    
    public AccountId FollowerId { get; set; }
    public Account Follower { get; set; } = null!; 
    
    public AccountId FollowingId { get; set; }
    public Account FollowingAccount { get; set; } = null!; 

    public DateTime SubscribedAtUtc { get; set; } = DateTime.UtcNow;
}