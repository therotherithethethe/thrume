
using System;
using System.Collections.Generic;
using Thrume.Domain.EntityIds;

namespace Thrume.Domain.Entity;

public class Conversation
{
    public ConversationId Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedAt { get; set; }


    public virtual ICollection<Account> Participants { get; set; } = new List<Account>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}