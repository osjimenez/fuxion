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
    public class WindowStateToVisibilityConverter : IValueConverter
    {
        public WindowStateToVisibilityConverter()
        {
            MaximizedValue = Visibility.Visible;
            MinimizedValue = Visibility.Collapsed;
            NormalValue = Visibility.Visible;
        }
        public Visibility MaximizedValue { get; set; }
        public Visibility MinimizedValue { get; set; }
        public Visibility NormalValue { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is WindowState)) return null;
            var state = (WindowState)value;
            switch (state)
            {
                case WindowState.Maximized:
                    return MaximizedValue;
                case WindowState.Minimized:
                    return MinimizedValue;
                case WindowState.Normal:
                    return NormalValue;
                default:
                    throw new NotSupportedException($"The value '{state}' is not supported");
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException($"Method '{nameof(ConvertBack)}' is not implemented");
        }
    }
}
