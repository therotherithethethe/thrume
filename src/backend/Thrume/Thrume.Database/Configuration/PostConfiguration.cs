using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;

namespace Thrume.Database.Configuration;

public sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        const int maxPostLen = 280;
        builder.ToTable("posts");

        builder.Property(a => a.Id)
            .HasConversion(
                accId => accId.Value,
                guid => new PostId(guid)
            ).IsRequired();
        builder.Property(p => p.Content).HasMaxLength(maxPostLen);


        builder.HasMany(p => p.LikedBy)
            .WithMany(a => a.LikedPosts)
            .UsingEntity(j => j.ToTable("account_post_likes")); 
    }
}