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
    public class NullToVisibilityConverter : GenericConverter<object, Visibility>
    {
        public Visibility NullValue { get; set; } = Visibility.Collapsed;
        public Visibility NotNullValue { get; set; } = Visibility.Visible;

        public override Visibility Convert(object source, object parameter, CultureInfo culture)
        {
            return source == null ? NullValue : NotNullValue;
        }
        public override object ConvertBack(Visibility result, object parameter, CultureInfo culture)
        {
            if (result == NullValue) return null;
            throw new NotSupportedException($"The value '{result}' is not supported for 'ConvertBack' method");
        }
    }
}
