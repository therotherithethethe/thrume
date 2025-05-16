using System.ComponentModel;

namespace Thrume.Domain.EntityIds;

[TypeConverter(typeof(CommentIdTypeConverter))]
public readonly record struct CommentId(Guid Value)
{
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(CommentId id) => id.Value;
    public static implicit operator CommentId(Guid id) => new(id);
}