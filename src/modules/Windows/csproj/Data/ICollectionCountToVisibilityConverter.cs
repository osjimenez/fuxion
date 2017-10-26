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
    public sealed class ICollectionCountToVisibilityConverter : GenericConverter<ICollection,Visibility>
    {
        public Visibility ZeroValue { get; set; } = Visibility.Collapsed;
        public Visibility NotZeroValue { get; set; } = Visibility.Visible;
        public Visibility NullValue { get; set; } = Visibility.Collapsed;
        public override Visibility Convert(ICollection source, CultureInfo culture)
        {
            if (source == null) return NullValue;
            if (source.Count == 0) return ZeroValue;
            return NotZeroValue;
        }
    }
}
