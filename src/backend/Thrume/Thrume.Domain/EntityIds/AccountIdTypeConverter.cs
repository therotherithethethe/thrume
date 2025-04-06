using System.ComponentModel;
using System.Globalization;

namespace Thrume.Domain.EntityIds;

public sealed class AccountIdTypeConverter : TypeConverter 
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        //if (value is not string stringValue) return base.ConvertFrom(context, culture, value);
        // if (string.IsNullOrWhiteSpace(stringValue))
        // {
        //     throw new NotSupportedException("Cannot convert an empty string to AccountId.");
        // }
        if (Guid.TryParse(value as string, out Guid guid))
        {
            return new AccountId(guid);
        }
        throw new NotSupportedException($"Cannot convert '{value}' to AccountId.");
    }
}
