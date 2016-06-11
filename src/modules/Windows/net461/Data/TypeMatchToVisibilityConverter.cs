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
    public enum TypeMatchConverterDefaultType
    {
        String,
        Int,
        Long,
        UIElement
    }
    public class TypeMatchToVisibilityConverter : IValueConverter
    {
        public TypeMatchConverterDefaultType TargetType { get; set; }
        public Visibility Match { get; set; } = Visibility.Visible;
        public Visibility NotMatch { get; set; } = Visibility.Collapsed;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (TargetType)
            {
                case TypeMatchConverterDefaultType.String:
                    if (value is string) return Match;
                    break;
                case TypeMatchConverterDefaultType.Int:
                    if (value is int) return Match;
                    break;
                case TypeMatchConverterDefaultType.Long:
                    if (value is long) return Match;
                    break;
                case TypeMatchConverterDefaultType.UIElement:
                    if (value is UIElement) return Match;
                    break;
            }
            return NotMatch;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
