using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Fuxion.Windows.Data
{
    public enum TimeSpanToStringMode
    {
        Full,
        ThreeTenant,
        TwoTenant,
    }
    public class TimeSpanToStringConverter : IValueConverter
    {
        public TimeSpanToStringMode Mode { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan?)
            {
                var ts = ((TimeSpan?)value).Value;
                string res = "";
                switch (Mode)
                {
                    case TimeSpanToStringMode.Full:
                        res += $"{ts.Days} days, ";
                        res += $"{ts.Hours} hours, ";
                        res += $"{ts.Minutes} minutes, ";
                        return res.Trim(',', ' ');
                    case TimeSpanToStringMode.ThreeTenant:
                        if (ts.Days > 0) res += $"{ts.Days} days, ";
                        if (ts.Hours > 0) res += $"{ts.Hours} hours, ";
                        if (ts.Minutes > 0) res += $"{ts.Minutes} minutes, ";
                        return res.Trim(',', ' ');
                    case TimeSpanToStringMode.TwoTenant:
                        if (ts.Days > 0) res += $"{ts.Days} days, ";
                        if (ts.Hours > 0) res += $"{ts.Hours} hours, ";
                        if (ts.Minutes > 0) res += $"{ts.Minutes} minutes, ";
                        return res.Trim(',', ' ');
                }

            }
            throw new NotSupportedException($"The value '{value}' is not supported for '{nameof(Convert)}' method");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException($"Method '{nameof(ConvertBack)}' is not implemented");
        }
    }
}
