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
    public class WindowStateToVisibilityConverter : GenericConverter<WindowState,Visibility>
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
        public override Visibility Convert(WindowState source, object parameter, CultureInfo culture)
        {
            switch (source)
            {
                case WindowState.Maximized:
                    return MaximizedValue;
                case WindowState.Minimized:
                    return MinimizedValue;
                case WindowState.Normal:
                    return NormalValue;
                default:
                    throw new NotSupportedException($"The value '{source}' is not supported");
            }
        }
    }
}
