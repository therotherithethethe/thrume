using System.ComponentModel;

namespace Thrume.Domain.EntityIds;
[TypeConverter(typeof(PostIdTypeConverter))]
public readonly record struct PostId(Guid Value)
{
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(PostId id) => id.Value;
    public static implicit operator PostId(Guid id) => new(id);
}
