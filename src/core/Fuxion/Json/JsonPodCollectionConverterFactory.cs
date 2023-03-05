using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fuxion.Json;

public class JsonPodCollectionConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsSubclassOfRawGeneric(typeof(JsonPodCollection<>));
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var types = typeToConvert.GetGenericArguments();
		var converterType = typeof(JsonPodCollectionConverter<>).MakeGenericType(types[0]);
		return (JsonConverter)(Activator.CreateInstance(converterType) ?? throw new InvalidCastException($"'{converterType.GetSignature()}' could not be created"));
	}
}