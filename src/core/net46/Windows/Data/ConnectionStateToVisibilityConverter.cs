using Fuxion.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;

namespace Fuxion.Windows.Data
{
    public class ConnectionStateToVisibilityConverter : GenericConverter<ConnectionState, Visibility>
    {
        public Visibility CreatedValue { get; set; } = Visibility.Collapsed;
        public Visibility OpeningValue { get; set; } = Visibility.Collapsed;
        public Visibility OpenedValue { get; set; } = Visibility.Collapsed;
        public Visibility ClosingValue { get; set; } = Visibility.Collapsed;
        public Visibility ClosedValue { get; set; } = Visibility.Collapsed;
        public Visibility FaultedValue { get; set; } = Visibility.Collapsed;
        public override Visibility Convert(ConnectionState source, object parameter, CultureInfo culture)
        {
            switch (source)
            {
                case ConnectionState.Created:
                    return CreatedValue;
                case ConnectionState.Opening:
                    return OpeningValue;
                case ConnectionState.Opened:
                    return OpenedValue;
                case ConnectionState.Closing:
                    return ClosingValue;
                case ConnectionState.Closed:
                    return ClosedValue;
                case ConnectionState.Faulted:
                    return FaultedValue;
                default:
                    return Visibility.Visible;
            }
        }
    }
}
