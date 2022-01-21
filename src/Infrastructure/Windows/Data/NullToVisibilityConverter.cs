namespace Fuxion.Windows.Data;

using System.Globalization;
using System.Windows;

public class NullToVisibilityConverter : GenericConverter<object?, Visibility>
{
	public Visibility NullValue { get; set; } = Visibility.Collapsed;
	public Visibility NotNullValue { get; set; } = Visibility.Visible;

	public override Visibility Convert(object? source, CultureInfo culture) => source == null ? NullValue : NotNullValue;
	public override object? ConvertBack(Visibility result, CultureInfo culture)
	{
		if (result == NullValue) return null;
		throw new NotSupportedException($"The value '{result}' is not supported for 'ConvertBack' method");
	}
}