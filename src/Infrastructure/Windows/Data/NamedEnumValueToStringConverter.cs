namespace Fuxion.Windows.Data;

using System.Globalization;

public class NamedEnumValueToStringConverter : GenericConverter<NamedEnumValue, string>
{
	public override string Convert(NamedEnumValue source, CultureInfo culture) => source.ToString();
}