using System.ComponentModel;
using System.Globalization;

namespace Thrume.Domain.EntityIds;

public readonly record struct ImageId(string ImageUrl);

public sealed class ImageIdTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => 
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) => 
        new ImageId(value as string ?? throw new InvalidOperationException($"Cannot convert {value} as imageId"));
}