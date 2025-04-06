using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Thrume.Database.Configuration;
using Thrume.Domain;
using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;

namespace Thrume.Database;

public sealed class AppDbContext(DbContextOptions<AppDbContext> dbOptions) :
    IdentityDbContext<Account, IdentityRole<AccountId>, AccountId>(dbOptions)
{
    public DbSet<Account> AccountDbSet => base.Set<Account>();
    public DbSet<Post> PostDbSet => base.Set<Post>();
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
    }
}