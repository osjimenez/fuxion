using Fuxion.Windows.Resources;
using System;
using System.Globalization;

namespace Fuxion.Windows.Data
{
    public enum TimeSpanToStringMode
    {
        PerElements,
        PerElementsOnlyLetters,
        PerTotals,
        Ticks,
    }
    public class TimeSpanToStringConverter : GenericConverter<TimeSpan?,string?>
    {
        public TimeSpanToStringMode Mode { get; set; }
        public int NumberOfElements { get; set; } = 5;
        public override string? Convert(TimeSpan? source, CultureInfo culture)
        {
            if (source == null || !source.HasValue) return null;
            var ts = source.Value;
            string res = "";
            switch (Mode)
            {
                case TimeSpanToStringMode.PerElements:
                    return ts.ToTimeString(NumberOfElements);
                case TimeSpanToStringMode.PerElementsOnlyLetters:
                    return ts.ToTimeString(NumberOfElements, true);
                case TimeSpanToStringMode.Ticks:
                    res += $"{ts.Ticks} {(ts.Ticks > 1 ? Strings.ticks : Strings.tick)}, ";
                    return res;
                default:
                    return ts.ToString();
            }
        }
    }
}
