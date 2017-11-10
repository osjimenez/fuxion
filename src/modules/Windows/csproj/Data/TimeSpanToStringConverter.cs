using Fuxion.Windows.Resources;
using System;
using System.Globalization;

namespace Fuxion.Windows.Data
{
    public enum TimeSpanToStringMode
    {
        PerElements,
        PerTotals,
        Ticks,
    }
    public class TimeSpanToStringConverter : GenericConverter<TimeSpan?,string>
    {
        public TimeSpanToStringMode Mode { get; set; }
        public int NumberOfElements { get; set; } = 5;
        public override string Convert(TimeSpan? source, CultureInfo culture)
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
                        res += $"{ts.Days} {(ts.Days > 1 ? Strings.days : Strings.day)}, ";
                        count++;
                    }
                    if (count >= NumberOfElements) return res.Trim(',', ' ');
                    if (ts.Hours > 0)
                    {
                        res += $"{ts.Hours} {(ts.Hours > 1 ? Strings.hours : Strings.hour)}, ";
                        count++;
                    }
                    if (count >= NumberOfElements) return res.Trim(',', ' ');
                    if (ts.Minutes > 0)
                    {
                        res += $"{ts.Minutes} {(ts.Minutes > 1 ? Strings.minutes : Strings.minute)}, ";
                        count++;
                    }
                    if (count >= NumberOfElements) return res.Trim(',', ' ');
                    if (ts.Seconds > 0)
                    {
                        res += $"{ts.Seconds} {(ts.Seconds > 1 ? Strings.seconds : Strings.minute)}, ";
                        count++;
                    }
                    if (count >= NumberOfElements) return res.Trim(',', ' ');
                    if (ts.Milliseconds > 0)
                    {
                        res += $"{ts.Milliseconds} {(ts.Milliseconds > 1 ? Strings.milliseconds : Strings.millisecond)}, ";
                        count++;
                    }
                    if (count >= NumberOfElements) return res.Trim(',', ' ');
                    //if (ts.Ticks > 0)
                    //{
                    //    res += $"{ts.Ticks} {(ts.Ticks > 1 ? Strings.ticks : Strings.tick)}, ";
                    //    count++;
                    //}
                    //if (count >= NumberOfElements) return res.Trim(',', ' ');
                    return res.Trim(',', ' ');
                case TimeSpanToStringMode.Ticks:
                    res += $"{ts.Ticks} {(ts.Ticks > 1 ? Strings.ticks : Strings.tick)}, ";
                    return res;
                default:
                    return ts.ToString();
            }
        }
    }
}
