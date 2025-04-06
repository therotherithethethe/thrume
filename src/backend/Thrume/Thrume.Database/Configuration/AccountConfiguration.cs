using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;

namespace Thrume.Database.Configuration;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
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
            .IsRequired();                   

        builder.Property(a => a.Email)       
            .IsRequired();                     

        builder.Property(a => a.EmailConfirmed)
            .HasDefaultValue(true)
            .IsRequired();                      

        builder.HasMany(a => a.Posts).WithOne(a => a.Author).HasForeignKey("author_id");
        
        builder.Property(a => a.PictureUrl).HasMaxLength(100);
        
    }
}