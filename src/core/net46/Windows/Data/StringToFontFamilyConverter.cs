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
    public class StringToFontFamilyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                return new FontFamily(value.ToString());
            }
            throw new NotSupportedException($"The value '{value}' is not supported for '{nameof(Convert)}' method");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new FontFamily(value.ToString());
        }
    }
}
