namespace Fuxion.Windows.Data;

using System.Globalization;
using System.Windows;

public enum BooleanMultiConverterMode
{
	AllTrue, AnyTrue, AllFalse, AnyFalse
}
public sealed class BooleanToVisibilityMultiConverter : GenericMultiConverter<bool, Visibility>
{
	public BooleanMultiConverterMode Mode { get; set; } = BooleanMultiConverterMode.AllTrue;
	public Visibility TrueValue { get; set; } = Visibility.Visible;
	public Visibility FalseValue { get; set; } = Visibility.Collapsed;
	public Visibility EmptyValue { get; set; } = Visibility.Collapsed;
	public Visibility NullValue { get; set; } = Visibility.Collapsed;
	//public bool AllowNullValues { get; set; }
	//public bool NullValue { get; set; }
	public override Visibility Convert(bool[] source, CultureInfo culture)
	{
		if (source == null) return NullValue;
		if (!source.Any()) return EmptyValue;
		switch (Mode)
		{
			case BooleanMultiConverterMode.AllTrue:
				return source.Any(v => !v) ? FalseValue : TrueValue;
			case BooleanMultiConverterMode.AnyTrue:
				return source.Any(v => v) ? TrueValue : FalseValue;
			case BooleanMultiConverterMode.AllFalse:
				return source.Any(v => v) ? FalseValue : TrueValue;
			case BooleanMultiConverterMode.AnyFalse:
				return source.Any(v => !v) ? TrueValue : FalseValue;
			default:
				throw new NotSupportedException($"The value of Mode '{Mode}' is not supported");
		}
	}
}