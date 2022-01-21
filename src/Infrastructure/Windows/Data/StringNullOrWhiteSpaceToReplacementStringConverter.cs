namespace Fuxion.Windows.Data;

using System.Globalization;

public class StringNullOrWhiteSpaceToReplacementStringConverter : GenericConverter<string, string>
{
	public string NullValue { get; set; } = "null";
	public string WhiteSpaceValue { get; set; } = "empty";
	public string EmptyValue { get; set; } = "empty";
	public override string Convert(string source, CultureInfo culture)
	{
		if (source == null) return NullValue;
		if (source == "") return EmptyValue;
		return string.IsNullOrWhiteSpace(source)
			? WhiteSpaceValue
			: source;
	}
}