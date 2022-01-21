namespace Fuxion.Windows.Data;

using System.Globalization;
using System.Windows.Media;

public class StringToFontFamilyConverter : GenericConverter<string, FontFamily>
{
	public override FontFamily Convert(string source, CultureInfo culture) => new FontFamily(source);
	public override string ConvertBack(FontFamily result, CultureInfo culture) => result.ToString();
}