using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Fuxion.Windows.Data
{
    public class StringToFontFamilyConverter : GenericConverter<string, FontFamily>
    {
        public override FontFamily Convert(string source, object parameter, CultureInfo culture)
        {
            return new FontFamily(source);
        }
        public override string ConvertBack(FontFamily result, object parameter, CultureInfo culture)
        {
            return result.ToString();
        }
    }
}
