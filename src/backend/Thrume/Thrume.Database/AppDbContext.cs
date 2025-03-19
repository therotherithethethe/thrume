using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Thrume.Domain;

namespace Thrume.Database;

public class AppDbContext(DbContextOptions<AppDbContext> dbOptions) :
    IdentityDbContext<Account, IdentityRole<AccountId>, AccountId>(dbOptions)
{
    public DbSet<Account> AccountDbSet => base.Set<Account>();
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
    }
}