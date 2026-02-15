using System.Globalization;
using Microsoft.Maui.Controls;

namespace MicroERP.App.SwiftServe.Components.MauiPages.Converters;

/// <summary>
/// IsAvailable: 0 = Available (green), 1 = Busy (red).
/// </summary>
public class IsAvailableToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue)
            return intValue == 1 ? Color.FromArgb("#e74c3c") : Color.FromArgb("#51cf66");
        return Color.FromArgb("#51cf66");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
