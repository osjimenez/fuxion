using System.Globalization;
using System.Windows.Media;

namespace Fuxion.Windows.Data;

public class StringToFontFamilyConverter : GenericConverter<string, FontFamily>
{
	public override FontFamily Convert(string source, CultureInfo culture) => new(source);
	public override string ConvertBack(FontFamily result, CultureInfo culture) => result.ToString();
}