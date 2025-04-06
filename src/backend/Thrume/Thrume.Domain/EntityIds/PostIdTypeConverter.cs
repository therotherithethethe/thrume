using System.ComponentModel;
using System.Globalization;

namespace Thrume.Domain.EntityIds;

public sealed class PostIdTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (!Guid.TryParse(value as string, out var guid)) throw new NotSupportedException($"Cannot convert '{value}' to PostId.");
        return new PostId(guid);
    }
}
