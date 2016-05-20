using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Fuxion.Windows.Data
{
    public sealed class NullToBooleanConverter : IValueConverter
    {
        public bool NullValue { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null) ? NullValue : !NullValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
                if (((bool)value) != NullValue) return null;
            throw new NotSupportedException($"The value '{value}' is not supported for 'ConvertBack' method");
        }
    }
}
