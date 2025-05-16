using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;

namespace Thrume.Database.Configuration;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever(); 
        builder.Property(c => c.Id)
            .HasConversion(
                c => c.Value,
                v => new CommentId(v));

        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(1000); 

        builder.Property(c => c.CreatedAt)
            .IsRequired();


        builder.HasOne(c => c.Author)
            .WithMany() 
            .HasForeignKey(c => c.AuthorId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade); 


        builder.HasOne(c => c.Post)
            .WithMany(p => p.Comments) 
            .HasForeignKey(c => c.PostId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies) 
            .HasForeignKey(c => c.ParentCommentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.ClientSetNull);
                                                     
    }
}