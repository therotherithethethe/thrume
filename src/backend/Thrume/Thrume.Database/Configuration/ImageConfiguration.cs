using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;

namespace Thrume.Database.Configuration;

public sealed class ImageConfiguration : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.ToTable("images");
        builder.Property(i => i.Id)
            .HasConversion(
                id => id.ImageUrl,
                url => new ImageId(url))
            .IsRequired();
        builder
            .HasOne(a => a.Post)
            .WithMany(p => p.Images);
    }
}