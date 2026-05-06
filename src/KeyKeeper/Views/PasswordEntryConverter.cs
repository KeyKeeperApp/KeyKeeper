using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using KeyKeeper.PasswordStore;

namespace KeyKeeper.Views;

public class PasswordEntryConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        PassStoreEntry? entry = value as PassStoreEntry;
        if (entry is PassStoreEntryLink link)
        {
            entry = link.LinkTarget;
        }

        if (entry == null)
            return value;

        if (parameter is string propertyPath && !string.IsNullOrEmpty(propertyPath))
        {
            try
            {
                object? current = entry;
                foreach (var prop in propertyPath.Split('.'))
                {
                    if (current == null)
                        return null;
                    var propInfo = current.GetType().GetProperty(prop);
                    current = propInfo?.GetValue(current);
                }
                return current;
            }
            catch
            {
                return value;
            }
        }

        return entry;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingNotification.UnsetValue;
    }
}