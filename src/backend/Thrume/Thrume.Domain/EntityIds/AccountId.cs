using System.ComponentModel;

namespace Thrume.Domain.EntityIds;
[TypeConverter(typeof(AccountIdTypeConverter))]
//TODO: do i have another way to do this?
public readonly record struct AccountId(Guid Value)
{
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(AccountId id) => id.Value;
    public static implicit operator AccountId(Guid id) => new(id);
}
