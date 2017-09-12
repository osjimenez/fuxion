using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Fuxion.Windows.Data
{
    public class NullableBooleanToVisibilityConverter : GenericConverter<bool?, Visibility>
    {
        public Visibility TrueValue { get; set; } = Visibility.Visible;
        public Visibility FalseValue { get; set; } = Visibility.Collapsed;
        public Visibility NullValue { get; set; } = Visibility.Collapsed;

        public override Visibility Convert(bool? source, object parameter, CultureInfo culture)
        {
            if (source == null || !source.HasValue) return NullValue;
            return source.Value ? TrueValue : FalseValue;
        }
        public override bool? ConvertBack(Visibility result, object parameter, CultureInfo culture)
        {
            if (result == TrueValue) return true;
            if (result == FalseValue) return false;
            if (result == NullValue) return null;
            throw new NotSupportedException($"The value '{result}' is not supported for '{nameof(ConvertBack)}' method");
        }
    }
}
