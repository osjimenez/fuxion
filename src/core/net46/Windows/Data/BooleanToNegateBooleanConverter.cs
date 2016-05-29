using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Fuxion.Windows.Data
{
    public class BooleanToNegateBooleanConverter : GenericConverter<bool, bool>
    {
        public override bool Convert(bool source, object parameter, CultureInfo culture)
        {
            return !source;
        }
        public override bool ConvertBack(bool result, object parameter, CultureInfo culture)
        {
            return !result;
        }
    }
}
