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
    public class BooleanToVisibilityConverter : GenericConverter<bool,Visibility>
    {
        public Visibility TrueValue { get; set; } = Visibility.Visible;
        public Visibility FalseValue { get; set; } = Visibility.Collapsed;

        public override Visibility Convert(bool source, object parameter, CultureInfo culture)
        {
            return source ? TrueValue : FalseValue;
        }
        public override bool ConvertBack(Visibility result, object parameter, CultureInfo culture)
        {
            if (result == TrueValue) return true;
            if (result == FalseValue) return false;
            throw new NotSupportedException($"The value '{result}' is not supported for '{nameof(ConvertBack)}' method");
        }
    }
}
