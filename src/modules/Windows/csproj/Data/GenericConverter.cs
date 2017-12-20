using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Reflection;
using System.Windows;

namespace Fuxion.Windows.Data
{
    public abstract class GenericConverter<TSource, TResult> : IValueConverter
    {
        public bool AllowUnsetValue { get; set; }
        public TResult UnsetValue { get; set; }
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // if allow unset value
            if (AllowUnsetValue && value == DependencyProperty.UnsetValue) return UnsetValue;
            if (value == DependencyProperty.UnsetValue) throw new NotSupportedException($"The value is DependencyProperty.UnsetValue. To support unset values use '{nameof(AllowUnsetValue)}' property of the '{GetType().Name}' class");
            // value must be TSource, call Convert
            // value is null and TSource is nullable, call Convert
            if (value is TSource
                ||
                (value == null && typeof(TSource).IsNullable()))
                return Convert((TSource)value, culture);
            // value is null and TResult is nullable, return null
            if (value == null && typeof(TResult).IsNullable())
                return null;
            // In any other case, value is not supported exception
            throw new NotSupportedException($"The value '{value}' is not supported for '{nameof(Convert)}' method, must be of type '{typeof(TSource).Name}'");
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value must be TSource, call ConvertBack
            // value is null and TResult is nullable, call ConvertBack
            if (value is TResult
                ||
                (value == null && typeof(TResult).IsNullable()))
                return ConvertBack((TResult)value, culture);
            // value is null and Tsource is nullable, return null
            if (value == null && typeof(TSource).IsNullable())
                return null;
            // In any other case, value is not supported exception
            throw new NotSupportedException($"The value '{value}' is not supported for '{nameof(ConvertBack)}' method, must be of type '{typeof(TResult).Name}'");
        }
        public abstract TResult Convert(TSource source, CultureInfo culture);
        public virtual TSource ConvertBack(TResult result, CultureInfo culture)
            => throw new NotSupportedException($"'{this.GetType().GetMethod(nameof(ConvertBack)).GetSignature(includeReturn: true)}' method, is not supported");
    }
    public abstract class GenericConverter<TSource, TResult, TParameter> : IValueConverter
    {
        public bool AllowUnsetValue { get; set; }
        public TResult UnsetValue { get; set; }
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (typeof(TParameter) != typeof(object) && !(parameter is TParameter)) throw new NotSupportedException($"The parameter must be of type '{typeof(TParameter).Name}'");
            // if allow unset value
            if (AllowUnsetValue && value == DependencyProperty.UnsetValue) return UnsetValue;
            if (value == DependencyProperty.UnsetValue) throw new NotSupportedException($"The value is DependencyProperty.UnsetValue. To support unset values use '{nameof(AllowUnsetValue)}' property of the '{GetType().Name}' class");
            // value must be TSource, call Convert
            // value is null and TSource is nullable, call Convert
            if (value is TSource
                ||
                (value == null && typeof(TSource).IsNullable()))
                return Convert((TSource)value, (TParameter)parameter, culture);
            // value is null and TResult is nullable, return null
            if (value == null && typeof(TResult).IsNullable())
                return null;
            // In any other case, value is not supported exception
            throw new NotSupportedException($"The value '{value}' is not supported for '{nameof(Convert)}' method, must be of type '{typeof(TSource).Name}'");
        }
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (typeof(TParameter) != typeof(object) && !(parameter is TParameter)) throw new NotSupportedException($"The parameter must be of type '{typeof(TParameter).Name}'");
            // value must be TSource, call ConvertBack
            // value is null and TResult is nullable, call ConvertBack
            if (value is TResult
                ||
                (value == null && typeof(TResult).IsNullable()))
                return ConvertBack((TResult)value, (TParameter)parameter, culture);
            // value is null and Tsource is nullable, return null
            if (value == null && typeof(TSource).IsNullable())
                return null;
            // In any other case, value is not supported exception
            throw new NotSupportedException($"The value '{value}' is not supported for '{nameof(ConvertBack)}' method, must be of type '{typeof(TResult).Name}'");
        }
        public abstract TResult Convert(TSource source, TParameter parameter, CultureInfo culture);
        public virtual TSource ConvertBack(TResult result, TParameter parameter, CultureInfo culture)
            => throw new NotSupportedException($"'{this.GetType().GetMethod(nameof(ConvertBack)).GetSignature(includeReturn: true)}' method, is not supported");
    }
}
