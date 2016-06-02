using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Fuxion.Windows.Data
{
    public sealed class NullToBooleanConverter : GenericConverter<object,bool>
    {
        public bool NullValue { get; set; }
        public override bool Convert(object source, object parameter, CultureInfo culture)
        {
            return (source == null) ? NullValue : !NullValue;
        }
        public override object ConvertBack(bool result, object parameter, CultureInfo culture)
        {
            if (result == NullValue) return null;
            throw new NotSupportedException($"The value '{result}' is not supported for 'ConvertBack' method");
        }
    }
}
