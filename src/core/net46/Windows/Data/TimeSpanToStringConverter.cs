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
        OneTenant,
        TwoTenant,
        ThreeTenant,
    }
    public class TimeSpanToStringConverter : GenericConverter<TimeSpan?,string>
    {
        public TimeSpanToStringMode Mode { get; set; }
        public override string Convert(TimeSpan? source, object parameter, CultureInfo culture)
        {
            var ts = source.Value;
            string res = "";
            switch (Mode)
            {
                case TimeSpanToStringMode.OneTenant:
                    if (ts.Days > 0) return $"{ts.Days} days, ";
                    if (ts.Hours > 0) return $"{ts.Hours} hours, ";
                    if (ts.Minutes > 0) return $"{ts.Minutes} minutes, ";
                    return $"{ts.Seconds} seconds, ";
                case TimeSpanToStringMode.TwoTenant:
                    if (ts.Days > 0) res += $"{ts.Days} days, ";
                    if (ts.Hours > 0) res += $"{ts.Hours} hours, ";
                    if (ts.Minutes > 0) res += $"{ts.Minutes} minutes, ";
                    return res.Trim(',', ' ');
                case TimeSpanToStringMode.ThreeTenant:
                    if (ts.Days > 0) res += $"{ts.Days} days, ";
                    if (ts.Hours > 0) res += $"{ts.Hours} hours, ";
                    if (ts.Minutes > 0) res += $"{ts.Minutes} minutes, ";
                    return res.Trim(',', ' ');
                case TimeSpanToStringMode.Full:
                default:
                    res += $"{ts.Days} days, ";
                    res += $"{ts.Hours} hours, ";
                    res += $"{ts.Minutes} minutes, ";
                    return res.Trim(',', ' ');
            }
        }
    }
}
