using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Fuxion.Windows.Data
{
    public class BooleanToNegateBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return !((bool)value);
            }
            throw new NotSupportedException($"The value '{value}' is not supported for '{nameof(Convert)}' method");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return !((bool)value);
            }
            throw new NotSupportedException($"The value '{value}' is not supported for '{nameof(ConvertBack)}' method");
        }
    }
}
