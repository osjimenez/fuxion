using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Fuxion.Windows.Data
{
    public sealed class IListCountToVisibilityConverter : IValueConverter
    {
        public Visibility ZeroValue { get; set; } = Visibility.Collapsed;
        public Visibility NotZeroValue { get; set; } = Visibility.Visible;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is IList)
            {
                if ((value as IList).Count == 0) return ZeroValue;
                return NotZeroValue;
            }
            throw new NotSupportedException($"The value '{value}' is not supported for 'Convert' method");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException($"Method '{nameof(ConvertBack)}' is not implemented");
        }
    }
}
