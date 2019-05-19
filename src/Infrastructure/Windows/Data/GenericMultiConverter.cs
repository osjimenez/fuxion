using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace Fuxion.Windows.Data
{
	public abstract class GenericMultiConverter<TSource, TResult> : IMultiValueConverter
	{
		public GenericMultiConverter() { }
		public GenericMultiConverter(bool valueTypesAreNotNullables) : this() => this.valueTypesAreNotNullables = valueTypesAreNotNullables;

		private readonly bool valueTypesAreNotNullables = true;
		public bool AllowUnsetValues { get; set; }
		public bool IgnoreUnsetValues { get; set; } = true;
		public TResult UnsetValue { get; set; } = default!;
		object? IMultiValueConverter.Convert(object?[]? values, Type targetType, object parameter, CultureInfo culture)
		{
			// if allow any unset value
			if (AllowUnsetValues)
			{
				// if unset values must be ignored, remove it from values
				if (IgnoreUnsetValues) values = values.Where(v => v != DependencyProperty.UnsetValue).ToArray();
				else if (values.Any(v => v == DependencyProperty.UnsetValue)) return UnsetValue;
			}
			else
			{
				if (values.Any(v => v == DependencyProperty.UnsetValue)) throw new NotSupportedException($"Some values are DependencyProperty.UnsetValue. To support unset values use '{nameof(AllowUnsetValues)}' property of the '{GetType().Name}' class");
			}
			// value must be TSource, call Convert
			// value is null and TSource is nullable, call Convert
			if (
				values.All(value => value is TSource
				||
				(value == null && typeof(TSource).IsNullable(valueTypesAreNotNullables))
				))
				return Convert(values.Cast<TSource>().ToArray(), culture);
			// In any other case, value is not supported exception
			throw new NotSupportedException($"The values '{values.Aggregate("", (a, c) => a + ", " + c?.ToString(), a => a.Trim(',', ' '))}' are not supported for '{GetType().Name}.{nameof(Convert)}' method, all must be of type '{typeof(TSource).Name}'");
		}
		object?[]? IMultiValueConverter.ConvertBack(object? value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			// value must be TSource, call ConvertBack
			// value is null and TResult is nullable, call ConvertBack
			if (value is TResult
				||
				(value == null && typeof(TResult).IsNullable(valueTypesAreNotNullables)))
				return ConvertBack((TResult)value!, culture).Cast<object>().ToArray();
			// value is null and Tsource is nullable, return null
			if (value == null && typeof(TSource).IsNullable(valueTypesAreNotNullables))
				return null;
			// In any other case, value is not supported exception
			throw new NotSupportedException($"The value '{value}' is not supported for '{GetType().Name}.{nameof(ConvertBack)}' method, must be of type '{typeof(TResult).Name}'");
		}
		public abstract TResult Convert(TSource[] source, CultureInfo culture);
		public virtual TSource[] ConvertBack(TResult result, CultureInfo culture)
			=> throw new NotSupportedException($"'{GetType().GetMethod(nameof(ConvertBack)).GetSignature(includeReturn: true)}' method, is not supported");
	}
	public abstract class GenericMultiConverter<TSource, TResult, TParameter> : IMultiValueConverter
	{
		public GenericMultiConverter() { }
		public GenericMultiConverter(bool valueTypesAreNotNullables) : this() => this.valueTypesAreNotNullables = valueTypesAreNotNullables;

		private readonly bool valueTypesAreNotNullables = true;
		public bool AllowUnsetValues { get; set; }
		public bool IgnoreUnsetValues { get; set; } = true;
		public TResult UnsetValue { get; set; } = default!;
		object? IMultiValueConverter.Convert(object?[]? values, Type targetType, object parameter, CultureInfo culture)
		{
			if (typeof(TParameter) != typeof(object) && !(parameter is TParameter)) throw new NotSupportedException($"The parameter must be of type '{typeof(TParameter).Name}'");
			// if allow any unset value
			if (AllowUnsetValues)
			{
				// if unset values must be ignored, remove it from values
				if (IgnoreUnsetValues) values = values.Where(v => v != DependencyProperty.UnsetValue).ToArray();
				else if (values.Any(v => v == DependencyProperty.UnsetValue)) return UnsetValue;
			}
			else
			{
				if (values.Any(v => v == DependencyProperty.UnsetValue)) throw new NotSupportedException($"Some values are DependencyProperty.UnsetValue. To support unset values use '{nameof(AllowUnsetValues)}' property of the '{GetType().Name}' class");
			}
			// value must be TSource, call Convert
			// value is null and TSource is nullable, call Convert
			if (
				values.All(value => value is TSource
				||
				(value == null && typeof(TSource).IsNullable(valueTypesAreNotNullables))
				))
				return Convert(values.Cast<TSource>().ToArray(), (TParameter)parameter, culture);
			// In any other case, value is not supported exception
			throw new NotSupportedException($"The values '{values.Aggregate("", (a, c) => a + ", " + c?.ToString(), a => a.Trim(',', ' '))}' are not supported for '{GetType().Name}.{nameof(Convert)}' method, all must be of type '{typeof(TSource).Name}'");
		}
		object?[]? IMultiValueConverter.ConvertBack(object? value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			if (typeof(TParameter) != typeof(object) && !(parameter is TParameter)) throw new NotSupportedException($"The parameter must be of type '{typeof(TParameter).Name}'");
			// value must be TSource, call ConvertBack
			// value is null and TResult is nullable, call ConvertBack
			if (value is TResult
				||
				(value == null && typeof(TResult).IsNullable(valueTypesAreNotNullables)))
				return ConvertBack((TResult)value!, (TParameter)parameter, culture).Cast<object>().ToArray();
			// value is null and Tsource is nullable, return null
			if (value == null && typeof(TSource).IsNullable(valueTypesAreNotNullables))
				return null;
			// In any other case, value is not supported exception
			throw new NotSupportedException($"The value '{value}' is not supported for '{GetType().Name}.{nameof(ConvertBack)}' method, must be of type '{typeof(TResult).Name}'");
		}
		public abstract TResult Convert(TSource[] source, TParameter parameter, CultureInfo culture);
		public virtual TSource[] ConvertBack(TResult result, TParameter parameter, CultureInfo culture)
			=> throw new NotSupportedException($"'{GetType().GetMethod(nameof(ConvertBack)).GetSignature(includeReturn: true)}' method, is not supported");
	}
}