
using System;
using System.ComponentModel;
using System.Globalization;
using Thrume.Domain.EntityIds; 

namespace Thrume.Domain.EntityIds.Converters;

public class ConversationIdTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string stringValue)
        {
            if (Guid.TryParse(stringValue, out var guid))
            {
                return new ConversationId(guid);
            }
        }
        return base.ConvertFrom(context, culture, value);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is ConversationId conversationId)
        {
            return conversationId.Value.ToString();
        }
        return base.ConvertTo(context, culture, value, destinationType);
    }
}