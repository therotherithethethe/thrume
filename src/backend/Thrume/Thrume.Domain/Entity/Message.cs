
using System;
using Thrume.Domain.EntityIds;

namespace Thrume.Domain.Entity;

public class Message
{
    public MessageId Id { get; set; }
    public ConversationId ConversationId { get; set; }
    public virtual Conversation Conversation { get; set; } = null!;
    public AccountId SenderId { get; set; }
    public virtual Account Sender { get; set; } = null!; 
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset SentAt { get; set; }

}