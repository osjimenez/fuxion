using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fuxion.Json;

public class JsonPodConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsSubclassOfRawGeneric(typeof(JsonPod<,>));
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var types = typeToConvert.GetGenericArguments();
		var converterType = typeof(JsonPodConverter<,,>).MakeGenericType(typeof(JsonPod<,>).MakeGenericType(types), types[0], types[1]);
		return (JsonConverter)(Activator.CreateInstance(converterType) ?? throw new InvalidCastException($"'{converterType.GetSignature()}' can not be created"));
	}
}