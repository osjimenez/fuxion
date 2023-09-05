using System.Globalization;

namespace Fuxion.Windows.Data;

public class BooleanToNegateBooleanConverter : GenericConverter<bool, bool>
{
	public override bool Convert(bool source, CultureInfo culture) => !source;
	public override bool ConvertBack(bool result, CultureInfo culture) => !result;
}