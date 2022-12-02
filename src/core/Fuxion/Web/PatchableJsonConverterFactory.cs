using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fuxion.Web;

public class PatchableJsonConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsSubclassOfRawGeneric(typeof(Patchable<>));
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var types = typeToConvert.GetGenericArguments();
		var patchableType = typeof(Patchable<>).MakeGenericType(types);
		var converterType = typeof(PatchableJsonConverter<>).MakeGenericType(types);
		return (JsonConverter)(Activator.CreateInstance(converterType) ?? throw new InvalidCastException("PatchableJsonConverter<T> can not be created"));
	}
}
