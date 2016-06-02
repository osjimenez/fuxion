using Fuxion.Windows.Resources;
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
        PerElements,
        PerTotals,
    }
    public class TimeSpanToStringConverter : GenericConverter<TimeSpan?,string>
    {
        public TimeSpanToStringMode Mode { get; set; }
        public int NumberOfElements { get; set; } = 5;
        public override string Convert(TimeSpan? source, object parameter, CultureInfo culture)
        {
            if (source == null || !source.HasValue) return null;
            var ts = source.Value;
            string res = "";
            switch (Mode)
            {
                case TimeSpanToStringMode.PerElements:
                    int count = 0;
                    if (count >= NumberOfElements) return res.Trim(',', ' ');
                    if (ts.Days > 0)
                    {
                        res += $"{ts.Days} {Strings.days}, ";
                        count++;
                    }
                    if (count >= NumberOfElements) return res.Trim(',', ' ');
                    if (ts.Hours > 0)
                    {
                        res += $"{ts.Hours} {Strings.hours}, ";
                        count++;
                    }
                    if (count >= NumberOfElements) return res.Trim(',', ' ');
                    if (ts.Minutes > 0)
                    {
                        res += $"{ts.Minutes} {Strings.minutes}, ";
                        count++;
                    }
                    if (count >= NumberOfElements) return res.Trim(',', ' ');
                    if (ts.Seconds > 0)
                    {
                        res += $"{ts.Seconds} {Strings.seconds}, ";
                        count++;
                    }
                    if (count >= NumberOfElements) return res.Trim(',', ' ');
                    if (ts.Milliseconds > 0)
                    {
                        res += $"{ts.Milliseconds} {Strings.milliseconds}, ";
                        count++;
                    }
                    if (count >= NumberOfElements) return res.Trim(',', ' ');
                    if (ts.Ticks > 0)
                    {
                        res += $"{ts.Ticks} {Strings.ticks}, ";
                        count++;
                    }
                    if (count >= NumberOfElements) return res.Trim(',', ' ');
                    return res.Trim(',', ' ');
                default:
                    return ts.ToString();
            }
        }
    }
}
