using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Fuxion.Windows.Data
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public Visibility TrueValue { get; set; } = Visibility.Visible;
        public Visibility FalseValue { get; set; } = Visibility.Collapsed;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
                if ((bool)value)
                    return TrueValue;
                else
                    return FalseValue;
            throw new NotSupportedException($"The value '{value}' is not supported for 'Convert' method");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
            {
                if ((Visibility)value == TrueValue) return true;
                if ((Visibility)value == FalseValue) return false;
            }
            throw new NotSupportedException($"The value '{value}' is not supported for 'ConvertBack' method");
        }
    }
}
