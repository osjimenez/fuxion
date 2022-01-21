namespace Fuxion.Windows.Data;

using System.Globalization;

public class EnumToNamedEnumValueConverter : GenericConverter<Enum, NamedEnumValue>
{
	public override NamedEnumValue Convert(Enum source, CultureInfo culture) => new NamedEnumValue(source);
	public override Enum ConvertBack(NamedEnumValue result, CultureInfo culture) => result.Value;
}