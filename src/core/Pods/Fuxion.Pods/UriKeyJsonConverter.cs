using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fuxion.Pods;

public class UriKeyJsonConverter : JsonConverter<UriKey>
{
	public override UriKey? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> new(JsonSerializer.Deserialize<Uri>(ref reader, options) ?? throw new JsonException($"Deserialization of '{nameof(UriKey)}' fails. Deserialization was null."), true);
	public override void Write(Utf8JsonWriter writer, UriKey value, JsonSerializerOptions options) => JsonSerializer.Serialize(writer, value.Key, options);
}