using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fuxion.Json;

public class JsonPodConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsSubclassOfRawGeneric(typeof(JsonPod<,>));
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var types         = typeToConvert.GetGenericArguments();
		var podType       = typeof(JsonPod<,>).MakeGenericType(types);
		var converterType = typeof(JsonPodConverter<,,>).MakeGenericType(podType, types[0], types[1]);
		return (JsonConverter)(Activator.CreateInstance(converterType) ?? throw new InvalidCastException("JsonPodConverter<TPod, TPayload, TKey> can not be created"));
	}
}