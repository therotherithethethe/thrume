
namespace Thrume.Domain.EntityIds;


public readonly record struct MessageId(Guid Value)
{
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(MessageId id) => id.Value;
    public static implicit operator MessageId(Guid id) => new(id);


}