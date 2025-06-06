using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Thrume.Database.Configuration;
using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;

namespace Thrume.Database;

public sealed class AppDbContext(DbContextOptions<AppDbContext> dbOptions) :
    IdentityDbContext<Account, IdentityRole<AccountId>, AccountId>(dbOptions)
{
    public DbSet<Account> AccountDbSet => Set<Account>();
    public DbSet<Post> PostDbSet => Set<Post>();
    public DbSet<Image> ImageDbSet => Set<Image>();
    public DbSet<Comment> CommentDbSet => Set<Comment>(); 
    public DbSet<Conversation> ConversationDbSet => Set<Conversation>();
    public DbSet<Message> MessageDbSet => Set<Message>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<IdentityRole<AccountId>>()
            .Property(a => a.Id)
            .HasConversion(
                id => id.Value,
                guid => new AccountId(guid)
            );

        new AccountConfiguration().Configure(builder.Entity<Account>());
        new PostConfiguration().Configure(builder.Entity<Post>());
        new ImageConfiguration().Configure(builder.Entity<Image>());
        new CommentConfiguration().Configure(builder.Entity<Comment>()); 
        
        builder.ApplyConfiguration(new ConversationConfiguration());
        builder.ApplyConfiguration(new MessageConfiguration());
        builder.ApplyConfiguration(new SubscriptionConfiguration());
    }
}