using System.Globalization;

namespace Fuxion.Windows.Data;

public sealed class NullToBooleanConverter : GenericConverter<object?, bool>
{
	public          bool NullValue                                    { get; set; }
	public override bool Convert(object? source, CultureInfo culture) => source == null ? NullValue : !NullValue;
	public override object? ConvertBack(bool result, CultureInfo culture)
	{
		if (result == NullValue) return null;
		throw new NotSupportedException($"The value '{result}' is not supported for 'ConvertBack' method");
	}
}