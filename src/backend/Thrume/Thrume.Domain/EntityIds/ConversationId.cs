
using System;
using System.ComponentModel;
using Thrume.Domain.EntityIds.Converters; 

namespace Thrume.Domain.EntityIds;

[TypeConverter(typeof(ConversationIdTypeConverter))] 
public readonly record struct ConversationId(Guid Value)
{
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(ConversationId id) => id.Value;
    public static implicit operator ConversationId(Guid id) => new(id);


}