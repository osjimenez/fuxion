namespace Fuxion.Windows.Data;

using System.Globalization;

public class EnumTypeToNamedEnumValueListConverter : GenericConverter<object, List<NamedEnumValue>, Type>
{
	public bool IsAlphabeticallyOrdered { get; set; }
	public override List<NamedEnumValue> Convert(object _, Type enumType, CultureInfo culture)
	{
		var res = Enum.GetValues(enumType).Cast<Enum>().Select(e => new NamedEnumValue(e)).ToList();
		if (IsAlphabeticallyOrdered) res = res.OrderBy(e => e.Name).ToList();
		return res;
	}
}