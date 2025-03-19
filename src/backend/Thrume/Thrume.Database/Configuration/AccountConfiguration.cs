using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thrume.Domain;

namespace Thrume.Database;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.Property(a => a.Id)
            .HasConversion(
                accId => accId.Value,        
                guid => new AccountId(guid)  
            )
            .IsRequired();                   

        builder.Property(a => a.UserName)
            .HasColumnName("username")       
            .IsRequired();                   

        builder.Property(a => a.Email)
            .HasColumnName("email")            
            .IsRequired();                     

        builder.Property(a => a.EmailConfirmed)
            .HasColumnName("email_confirmed")
            .HasDefaultValue(true)
            .IsRequired();                      

        builder.Property(a => a.PasswordHash)
            .HasColumnName("password_hash")     
            .IsRequired();                      

        // builder.Ignore(a => a.NormalizedUserName);
        // builder.Ignore(a => a.NormalizedEmail);
        // builder.Ignore(a => a.SecurityStamp);
        // builder.Ignore(a => a.ConcurrencyStamp);
        // builder.Ignore(a => a.PhoneNumber);
        // builder.Ignore(a => a.PhoneNumberConfirmed);
        // builder.Ignore(a => a.TwoFactorEnabled);
        // builder.Ignore(a => a.LockoutEnd);
        // builder.Ignore(a => a.LockoutEnabled);
        // builder.Ignore(a => a.AccessFailedCount);
    }
}