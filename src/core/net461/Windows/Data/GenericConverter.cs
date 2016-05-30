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
            // value must be TSource, call Convert
            if (value is TSource)
                return Convert((TSource)value, parameter, culture);
            // value is null and TSource is nullable, call Convert
            if (value == null && typeof(TSource).IsClass || (typeof(TSource).IsGenericType && typeof(TSource).GetGenericTypeDefinition() == typeof(Nullable<>)))
                return Convert((TSource)value, parameter, culture);
            // value is null and TResult is nullable, return null
            if (value == null && typeof(TResult).IsClass || (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(Nullable<>)))
                return null;
            // In any other case, value is not supported exception
            throw new NotSupportedException($"The value '{value}' is not supported for '{nameof(Convert)}' method, must be of type '{typeof(TSource).Name}'");
        }
        public virtual TSource ConvertBack(TResult result, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"'{this.GetType().GetMethod(nameof(Convert)).GetSignature(includeReturn: true)}' method, is not supported");
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value must be TSource, call ConvertBack
            if (value is TResult)
                return ConvertBack((TResult)value, parameter, culture);
            // value is null and TResult is nullable, call ConvertBack
            if (value == null && typeof(TResult).IsClass || (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(Nullable<>)))
                return ConvertBack((TResult)value, parameter, culture);
            // value is null and Tsource is nullable, return null
            if (typeof(TSource).IsClass || (typeof(TSource).IsGenericType && typeof(TSource).GetGenericTypeDefinition() == typeof(Nullable<>)))
                return null;
            // In any other case, value is not supported exception
            throw new NotSupportedException($"The value '{value}' is not supported for '{nameof(ConvertBack)}' method, must be of type '{typeof(TResult).Name}'");
        }
    }
}
