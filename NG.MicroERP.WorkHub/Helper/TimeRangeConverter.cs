using System.Globalization;
using Microsoft.Maui.Controls;
using NG.MicroERP.Shared.Models;

namespace NG.MicroERP.WorkHub.Helper;

public class TimeRangeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is RosterModel roster)
        {
            if (!string.IsNullOrEmpty(roster.StartTime) && !string.IsNullOrEmpty(roster.EndTime))
            {
                return $"{roster.StartTime} - {roster.EndTime}";
            }
            else if (!string.IsNullOrEmpty(roster.StartTime))
            {
                return roster.StartTime;
            }
        }
        return "--:--";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
