namespace Fuxion.Windows.Data;

using System.Globalization;

public sealed class BooleanToBooleanMultiConverter : GenericMultiConverter<bool, bool>
{
	public BooleanMultiConverterMode Mode { get; set; } = BooleanMultiConverterMode.AllTrue;
	public bool TrueValue { get; set; } = true;
	public bool FalseValue { get; set; } = false;
	public bool EmptyValue { get; set; } = false;
	public bool NullValue { get; set; } = false;
	public override bool Convert(bool[] source, CultureInfo culture)
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