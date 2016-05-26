using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Reflection;
namespace Fuxion.Windows.Data
{
    public abstract class GenericConverter<TSource, TResult> : IValueConverter
    {
        public abstract TResult Convert(TSource source, object parameter, CultureInfo culture);
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(targetType != typeof(TResult))
                throw new NotSupportedException($"targetType is not supported for '{nameof(Convert)}' method, must be of type '{typeof(TResult).Name}'");
            if (value is TSource)
                return Convert((TSource)value, parameter, culture);
            throw new NotSupportedException($"The value '{value}' is not supported for '{nameof(Convert)}' method, must be of type '{typeof(TSource).Name}'");
        }
        public virtual TSource ConvertBack(TResult result, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"'{this.GetType().GetMethod(nameof(Convert)).GetSignature(includeReturn: true)}' method, is not supported");
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(TSource))
                throw new NotSupportedException($"targetType is not supported for '{nameof(ConvertBack)}' method, must be of type '{typeof(TSource).Name}'");
            if (value is TResult)
                return ConvertBack((TResult)value, parameter, culture);
            throw new NotSupportedException($"The value '{value}' is not supported for '{nameof(ConvertBack)}' method, must be of type '{typeof(TResult).Name}'");
        }
    }
}
