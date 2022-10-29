using System.Globalization;

namespace Fuxion.Windows.Data;

public class NullableEnumToNamedEnumValueConverter : GenericConverter<Enum, NamedEnumValue?>
{
	public NullableEnumToNamedEnumValueConverter() : base(false) { }
	public override NamedEnumValue? Convert(Enum                source, CultureInfo culture) => source == null ? default : new NamedEnumValue(source);
	public override Enum            ConvertBack(NamedEnumValue? result, CultureInfo culture) => result ?? null!;
}