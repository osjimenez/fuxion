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
    public abstract class GenericConverter<TSource, TResult> : GenericConverter<TSource, TResult, object> { }
    public abstract class GenericConverter<TSource, TResult, TParameter> : IValueConverter
    {
        public abstract TResult Convert(TSource source, TParameter parameter, CultureInfo culture);
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (typeof(TParameter) != typeof(object) && !(parameter is TParameter)) throw new NotSupportedException($"The parameter must be of type '{typeof(TParameter).Name}'");
            // value must be TSource, call Convert
            if (value is TSource)
                return Convert((TSource)value, (TParameter)parameter, culture);
            // value is null and TSource is nullable, call Convert
            if (value == null && typeof(TSource).IsNullable())
                return Convert((TSource)value, (TParameter)parameter, culture);
            // value is null and TResult is nullable, return null
            if (value == null && typeof(TResult).IsNullable())
                return null;
            // In any other case, value is not supported exception
            throw new NotSupportedException($"The value '{value}' is not supported for '{nameof(Convert)}' method, must be of type '{typeof(TSource).Name}'");
        }
        public virtual TSource ConvertBack(TResult result, TParameter parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"'{this.GetType().GetMethod(nameof(ConvertBack)).GetSignature(includeReturn: true)}' method, is not supported");
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (typeof(TParameter) != typeof(object) && !(parameter is TParameter)) throw new NotSupportedException($"The parameter must be of type '{typeof(TParameter).Name}'");
            // value must be TSource, call ConvertBack
            if (value is TResult)
                return ConvertBack((TResult)value, (TParameter)parameter, culture);
            // value is null and TResult is nullable, call ConvertBack
            if (value == null && typeof(TResult).IsNullable())
                return ConvertBack((TResult)value, (TParameter)parameter, culture);
            // value is null and Tsource is nullable, return null
            if (value == null && typeof(TSource).IsNullable())
                return null;
            // In any other case, value is not supported exception
            throw new NotSupportedException($"The value '{value}' is not supported for '{nameof(ConvertBack)}' method, must be of type '{typeof(TResult).Name}'");
        }
    }
}
