using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;

namespace Fuxion.Windows.Data;

[ContentProperty(nameof(Converters))]
public class PipeConverter : IValueConverter
{
	public PipeConverterParameterMode ParameterMode { get; set; }
	public string ParameterSeparator { get; set; } = "|";
	public ObservableCollection<IValueConverter> Converters { get; } = new();
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		var output = value;
		for (var i = 0; i < Converters.Count; ++i)
		{
			var target = i == Converters.Count - 1 ? targetType : GetConverterTypes(Converters[i + 1]).SourceType;
			output = Converters[i].Convert(output, target, GetConverterParameter(Converters[i], parameter), culture);

			// If the converter returns 'DoNothing' then the binding operation should terminate.
			if (output == Binding.DoNothing) break;
		}
		return output;
	}
	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
	public (Type? SourceType, Type? TargetType, Type? ParameterType) GetConverterTypes(IValueConverter converter)
	{
		var att = converter.GetType().GetCustomAttribute<ValueConversionAttribute>(true, false);
		if (att != null) return (att.SourceType, att.TargetType, att.ParameterType);
		if (converter.GetType().IsSubclassOfRawGeneric(typeof(GenericConverter<,>)))
		{
			var args = converter.GetType().GetSubclassOfRawGeneric(typeof(GenericConverter<,>))!.GetGenericArguments();
			return (args[0], args[1], null);
		}
		if (converter.GetType().IsSubclassOfRawGeneric(typeof(GenericConverter<,,>)))
		{
			var args = converter.GetType().GetSubclassOfRawGeneric(typeof(GenericConverter<,,>))!.GetGenericArguments();
			return (args[0], args[1], args[2]);
		}
		return (null, null, null);
	}
	public object? GetConverterParameter(IValueConverter converter, object? parameter)
	{
		if (ParameterMode == PipeConverterParameterMode.AllSame || parameter == null) return parameter;
		var pars = parameter?.ToString()?.Split(new[] {
			ParameterSeparator
		}, StringSplitOptions.None) ?? new string[]
			{ };
		var index = Converters.IndexOf(converter);
		if (pars.Length <= index) throw new ArgumentOutOfRangeException("PipeConverter parameter was not define properly, has less parameters than converters");
		var val = pars[index];
		var parType = GetConverterTypes(converter).ParameterType;
		if (parType == null) return val;
		return TypeDescriptor.GetConverter(parType).ConvertFrom(val);
	}
}

public enum PipeConverterParameterMode
{
	AllSame,
	Individual
}

/// <summary>
///    A value converter which contains a list of IValueConverters and invokes their Convert or ConvertBack methods
///    in the order that they exist in the list.  The output of one converter is piped into the next converter
///    allowing for modular value converters to be chained together.  If the ConvertBack method is invoked, the
///    value converters are executed in reverse order (highest to lowest index).  Do not leave an element in the
///    Converters property collection null, every element must reference a valid IValueConverter instance. If a
///    value converter's type is not decorated with the ValueConversionAttribute, an InvalidOperationException will be
///    thrown when the converter is added to the Converters collection.
/// </summary>
[ContentProperty("Converters")]
public class ValueConverterGroup : IValueConverter
{
	public ValueConverterGroup() => Converters.CollectionChanged += OnConvertersCollectionChanged;
	readonly Dictionary<IValueConverter, ValueConversionAttribute> cachedAttributes = new();
	public ObservableCollection<IValueConverter> Converters { get; } = new();
	object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		var output = value;
		for (var i = 0; i < Converters.Count; ++i)
		{
			var converter = Converters[i];
			var currentTargetType = GetTargetType(i, targetType, true);
			output = converter.Convert(output, currentTargetType, parameter, culture);

			// If the converter returns 'DoNothing' then the binding operation should terminate.
			if (output == Binding.DoNothing) break;
		}
		return output;
	}
	object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		var output = value;
		for (var i = Converters.Count - 1; i > -1; --i)
		{
			var converter = Converters[i];
			var currentTargetType = GetTargetType(i, targetType, false);
			output = converter.ConvertBack(output, currentTargetType, parameter, culture);

			// When a converter returns 'DoNothing' the binding operation should terminate.
			if (output == Binding.DoNothing) break;
		}
		return output;
	}
	protected virtual Type GetTargetType(int converterIndex, Type finalTargetType, bool convert)
	{
		// If the current converter is not the last/first in the list, 
		// get a reference to the next/previous converter.
		IValueConverter? nextConverter = null;
		if (convert)
		{
			if (converterIndex < Converters.Count - 1)
			{
				nextConverter = Converters[converterIndex + 1];
				if (nextConverter == null) throw new InvalidOperationException("The Converters collection of the ValueConverterGroup contains a null reference at index: " + (converterIndex + 1));
			}
		} else
		{
			if (converterIndex > 0)
			{
				nextConverter = Converters[converterIndex - 1];
				if (nextConverter == null) throw new InvalidOperationException("The Converters collection of the ValueConverterGroup contains a null reference at index: " + (converterIndex - 1));
			}
		}
		if (nextConverter != null)
		{
			var conversionAttribute = cachedAttributes[nextConverter];

			// If the Convert method is going to be called, we need to use the SourceType of the next 
			// converter in the list.  If ConvertBack is called, use the TargetType.
			return convert ? conversionAttribute.SourceType : conversionAttribute.TargetType;
		}

		// If the current converter is the last one to be executed return the target type passed into the conversion method.
		return finalTargetType;
	}
	void OnConvertersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		// The 'Converters' collection has been modified, so validate that each value converter it now
		// contains is decorated with ValueConversionAttribute and then cache the attribute value.
		IList convertersToProcess = new List<object>();
		if (e.NewItems is not null && (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace))
			convertersToProcess = e.NewItems;
		else if (e.OldItems is not null && e.Action == NotifyCollectionChangedAction.Remove)
			foreach (IValueConverter? converter in e.OldItems)
			{
				if (converter != null) cachedAttributes.Remove(converter);
			}
		else if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			cachedAttributes.Clear();
			convertersToProcess = Converters;
		}
		foreach (IValueConverter? converter in convertersToProcess)
			if (converter != null)
			{
				var attributes = converter.GetType().GetCustomAttributes<ValueConversionAttribute>(false).ToList();
				if (attributes.Count != 1)
					throw new InvalidOperationException("All value converters added to a ValueConverterGroup must be decorated with the ValueConversionAttribute attribute exactly once.");
				cachedAttributes.Add(converter, attributes[0]);
			}
	}
}