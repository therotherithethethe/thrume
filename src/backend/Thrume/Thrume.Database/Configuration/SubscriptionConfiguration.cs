using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thrume.Domain.Entity;

namespace Thrume.Database.Configuration;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.HasOne(s => s.Follower)
            .WithMany(a => a.Followers)
            .HasForeignKey(s => s.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(s => s.FollowingAccount)
            .WithMany(a => a.Following)
            .HasForeignKey(s => s.FollowingId)
            .OnDelete(DeleteBehavior.Restrict); 
        
        builder.HasIndex(s => new { s.FollowerId, s.FollowingId }).IsUnique();
    }
}
