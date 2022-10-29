using System.Globalization;

namespace Fuxion.Windows.Data;

public class EnumToNamedEnumValueConverter : GenericConverter<Enum, NamedEnumValue>
{
	public override NamedEnumValue Convert(Enum               source, CultureInfo culture) => new(source);
	public override Enum           ConvertBack(NamedEnumValue result, CultureInfo culture) => result.Value;
}