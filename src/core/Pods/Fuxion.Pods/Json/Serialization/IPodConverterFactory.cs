using System.Text.Json;
using System.Text.Json.Serialization;
using PodType = Fuxion.Pods.IPod<string, string>;

namespace Fuxion.Pods.Json.Serialization;

public class IPodConverterFactory(IUriKeyResolver? resolver = null) : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsSubclassOfRawGeneric(typeof(IPod<,>));
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var podType = typeToConvert.GetSubclassOfRawGeneric(typeof(IPod<,>));
		var disType = podType?.GetProperty(nameof(PodType.Discriminator))
			?.PropertyType;
		var payType = podType?.GetProperty(nameof(PodType.Payload))
			?.PropertyType;
		if (disType is null || payType is null) throw new ApplicationException($"Cannot determine types for discriminator and payload of type '{typeToConvert.GetSignature()}'");
		var converterType = typeof(IPodConverter<,,>).MakeGenericType(typeToConvert, disType, payType);
		return (JsonConverter)(Activator.CreateInstance(converterType, resolver) ?? throw new InvalidCastException($"'{converterType.GetSignature()}' can not be created"));
	}
}
public class JsonFactory<TFactory> : JsonConverterAttribute
{
	public JsonFactory(params object?[] arguments)
	{
		Arguments = arguments;
	}
	public object?[] Arguments { get; set; }
	public override JsonConverter? CreateConverter(Type typeToConvert)
	{
		return (JsonConverterFactory)(Activator.CreateInstance(typeof(TFactory), Arguments)
			?? throw new InvalidCastException($"'{typeof(TFactory).GetSignature()}' can not be created"));
	}
}
// https://github.com/dotnet/runtime/issues/54187#issuecomment-871293887
public class IPodJsonConverterFactoryAttribute : JsonConverterAttribute
{
	//public IPodConverterAttribute(Type converterType, params object?[] converterArguments)
	//{
	//	ConverterType = converterType;
	//	ConverterArguments = converterArguments;
	//}

	// CreateConverter method is only called if base.ConverterType is null 
	// so store the type parameter in a new property in the derived attribute
	// https://github.com/dotnet/runtime/blob/57bfe474518ab5b7cfe6bf7424a79ce3af9d6657/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/JsonSerializerOptions.Converters.cs#L278
	//public new Type ConverterType { get; }
	//public object?[] ConverterArguments { get; }

	public override JsonConverter? CreateConverter(Type typeToConvert)
	{
		var podType = typeToConvert.GetSubclassOfRawGeneric(typeof(IPod<,>));
		var disType = podType?.GetProperty(nameof(PodType.Discriminator))
			?.PropertyType;
		var payType = podType?.GetProperty(nameof(PodType.Payload))
			?.PropertyType;
		if (disType is null || payType is null) throw new ApplicationException($"Cannot determine types for discriminator and payload of type '{typeToConvert.GetSignature()}'");
		var converterType = typeof(IPodConverter<,,>).MakeGenericType(typeToConvert, disType, payType);
		return (JsonConverter)(Activator.CreateInstance(converterType, (IUriKeyResolver?)null) ?? throw new InvalidCastException($"'{converterType.GetSignature()}' can not be created"));
		return new IPodConverterFactory();
		//return (JsonConverter)Activator.CreateInstance(ConverterType, ConverterArguments)!;
	}
}