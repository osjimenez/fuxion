namespace Fuxion.Windows.Data;

using System.Globalization;

public enum TimeSpanToLongValue
{
	Ticks,
	Milliseconds,
	Seconds,
	Minutes,
	Hours,
	Days,
}
public class TimeSpanToLongConverter : GenericConverter<TimeSpan?, long?>
{
	public TimeSpanToLongValue Value { get; set; }
	public bool ConvertToLongAsTotalCount { get; set; }
	public override long? Convert(TimeSpan? source, CultureInfo culture)
	{
		if (source == null || !source.HasValue)
			return null;

		switch (Value)
		{
			case TimeSpanToLongValue.Ticks:
				return source.Value.Ticks;
			case TimeSpanToLongValue.Milliseconds:
				return ConvertToLongAsTotalCount
					? (long)source.Value.TotalMilliseconds
					: source.Value.Milliseconds;
			case TimeSpanToLongValue.Seconds:
				return ConvertToLongAsTotalCount
					? (int)source.Value.TotalSeconds
					: source.Value.Seconds;
			case TimeSpanToLongValue.Minutes:
				return ConvertToLongAsTotalCount
					? (int)source.Value.TotalMinutes
					: source.Value.Minutes;
			case TimeSpanToLongValue.Hours:
				return ConvertToLongAsTotalCount
					? (int)source.Value.TotalHours
					: source.Value.Hours;
			case TimeSpanToLongValue.Days:
				return ConvertToLongAsTotalCount
					? (int)source.Value.TotalDays
					: source.Value.Days;
			default:
				return null;
		}
	}
	public override TimeSpan? ConvertBack(long? result, CultureInfo culture)
	{
		if (result == null || !result.HasValue)
			return null;
		switch (Value)
		{
			case TimeSpanToLongValue.Ticks:
				return TimeSpan.FromTicks(result.Value);
			case TimeSpanToLongValue.Milliseconds:
				return TimeSpan.FromMilliseconds(result.Value);
			case TimeSpanToLongValue.Seconds:
				return TimeSpan.FromSeconds(result.Value);
			case TimeSpanToLongValue.Minutes:
				return TimeSpan.FromMinutes(result.Value);
			case TimeSpanToLongValue.Hours:
				return TimeSpan.FromHours(result.Value);
			case TimeSpanToLongValue.Days:
				return TimeSpan.FromDays(result.Value);
			default:
				throw new NotSupportedException("");
		}
	}
}