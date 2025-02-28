using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Thrume.Domain;

namespace Thrume.Database;

public class AppDbContext(DbContextOptions<AppDbContext> dbOptions) : IdentityDbContext(dbOptions)
{
    public DbSet<Account> AccountDbSet { get; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Account>()
            .Property(a => a.Id)
            .HasConversion(
                accId => accId.Value,
                guid => new AccountId(guid)
                );
    }
}