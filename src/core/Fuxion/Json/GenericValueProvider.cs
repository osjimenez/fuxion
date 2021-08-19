namespace Fuxion.Json;

using Newtonsoft.Json.Serialization;
using System.Reflection;

public class GenericValueProvider<TSource, TDestination> : IValueProvider
{
	public GenericValueProvider(IValueProvider? valueProvider, PropertyInfo property, Func<TSource, TDestination> convertFunction)
	{
		this.valueProvider = valueProvider;
		this.property = property;
		this.convertFunction = convertFunction;
	}

	private readonly IValueProvider? valueProvider;
	private readonly PropertyInfo property;
	private readonly Func<TSource, TDestination> convertFunction;
	public object? GetValue(object target)
	{
		var result = valueProvider == null
			? property.GetValue(target)
			: valueProvider.GetValue(target);
		return result is TSource source
			? convertFunction(source)
			: result;
	}

	public void SetValue(object target, object? value)
	{
		if (valueProvider == null)
			property.SetValue(target, value);
		else
			valueProvider.SetValue(target, value);
	}
}