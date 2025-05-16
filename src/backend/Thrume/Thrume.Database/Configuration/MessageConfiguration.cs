
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;

namespace Thrume.Database.Configuration;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasConversion(
                id => id.Value,
                guid => new MessageId(guid));


        builder.Property(m => m.ConversationId).IsRequired();
        builder.HasOne(m => m.Conversation)
               .WithMany(c => c.Messages)
               .HasForeignKey(m => m.ConversationId)
               .OnDelete(DeleteBehavior.Cascade); 


        builder.Property(m => m.SenderId).IsRequired();
        builder.HasOne(m => m.Sender)
               .WithMany()
               .HasForeignKey(m => m.SenderId)
               .OnDelete(DeleteBehavior.Restrict);


        builder.Property(m => m.Content)
               .IsRequired()
               .HasMaxLength(2000);


        builder.Property(m => m.SentAt)
               .IsRequired();

        builder.HasIndex(m => m.ConversationId);
        builder.HasIndex(m => m.SentAt);
    }
}