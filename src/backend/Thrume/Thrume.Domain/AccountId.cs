using System.ComponentModel;

namespace Thrume.Domain;
[TypeConverter(typeof(AccountIdTypeConverter))]
public readonly record struct AccountId(Guid Value)
{
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(AccountId id) => id.Value;
    public static implicit operator AccountId(Guid id) => new(id);
}
