
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thrume.Domain.Entity;
using Thrume.Domain.EntityIds;

namespace Thrume.Database.Configuration;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {

        builder.HasKey(c => c.Id);


        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                guid => new ConversationId(guid));


        builder.Property(c => c.CreatedAt)
            .IsRequired();


        builder.HasMany(c => c.Participants)
               .WithMany(); 


        builder.HasMany(c => c.Messages)
               .WithOne(m => m.Conversation)
               .HasForeignKey(m => m.ConversationId)
               .OnDelete(DeleteBehavior.Cascade); 
    }
}