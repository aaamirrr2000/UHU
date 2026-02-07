using System.Globalization;
using Microsoft.Maui.Controls;

namespace NG.MicroERP.App.SwiftServe.Components.MauiPages.Converters;

public class InvertBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            // For IsAvailable: 1 = Busy (disabled), 0 = Available (enabled)
            return intValue == 0;
        }
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}
