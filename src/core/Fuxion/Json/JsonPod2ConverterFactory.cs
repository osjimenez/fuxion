using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using PodType = Fuxion.IPod2<string,string>;

namespace Fuxion.Json;

public class JsonPod2ConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsSubclassOfRawGeneric(typeof(JsonPod2<,>));
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var types = typeToConvert.GetGenericArguments();
		var converterType = typeof(JsonPod2Converter<,,>).MakeGenericType(typeof(JsonPod2<,>).MakeGenericType(types), types[0], types[1]);
		return (JsonConverter)(Activator.CreateInstance(converterType) ?? throw new InvalidCastException($"'{converterType.GetSignature()}' can not be created"));
	}
}
public class JsonPodNode2ConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsSubclassOfRawGeneric(typeof(IPod2<,>));
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var disType = typeToConvert.GetProperty(nameof(PodType.Discriminator))
			?.PropertyType;
		var payType = typeToConvert.GetProperty(nameof(PodType.Payload))
			?.PropertyType;
		if (disType is null || payType is null) throw new ApplicationException($"Cannot determine types for discriminator and payload of type '{typeToConvert.Name}'");
		var converterType = typeof(IPod2Converter<,,>).MakeGenericType(typeToConvert, disType, payType);
		return (JsonConverter)(Activator.CreateInstance(converterType) ?? throw new InvalidCastException($"'{converterType.GetSignature()}' can not be created"));
	}
}