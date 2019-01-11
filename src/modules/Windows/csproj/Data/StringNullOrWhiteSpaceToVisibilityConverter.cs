using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Fuxion.Windows.Data
{
    public class StringNullOrWhiteSpaceToVisibilityConverter : GenericConverter<string, Visibility>
    {
        public Visibility NullValue { get; set; } = Visibility.Collapsed;
        public Visibility WhiteSpaceValue { get; set; } = Visibility.Collapsed;
        public Visibility EmptyValue { get; set; } = Visibility.Collapsed;
        public Visibility OtherValue { get; set; } = Visibility.Visible;
        public override Visibility Convert(string source, CultureInfo culture)
        {
            if (source == null) return NullValue;
            if (source == "") return EmptyValue;
            return string.IsNullOrWhiteSpace(source)
                ? WhiteSpaceValue
                : OtherValue;
        }
    }
}
