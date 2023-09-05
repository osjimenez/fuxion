using System.Globalization;

namespace Fuxion.Windows.Data;

public class NamedEnumValueToStringConverter : GenericConverter<NamedEnumValue, string>
{
	public override string Convert(NamedEnumValue source, CultureInfo culture) => source.ToString();
}