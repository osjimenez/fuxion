using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Fuxion.Json;

public class JsonPodCollectionConverter<TDiscriminator> : JsonConverter<JsonPodCollection<TDiscriminator>>
	where TDiscriminator : notnull
{
	public override JsonPodCollection<TDiscriminator>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException($"The '{nameof(JsonPodCollection<TDiscriminator>)}' deserialization must be start with token '{JsonTokenType.StartArray}'");
		reader.Read();
		JsonPodCollection<TDiscriminator> res = new();
		while (reader.TokenType != JsonTokenType.EndArray)
		{
			var ele = JsonDocument.ParseValue(ref reader).RootElement;
			var pro = ele.GetProperty(nameof(JsonPod<int, int>.Discriminator));
			var dis = (TDiscriminator)(pro.Deserialize(typeof(TDiscriminator), options) ?? throw new JsonException($"Property '{nameof(JsonPod<int, int>.Discriminator)}' with kind '{pro.ValueKind}' could not be deserialized"));
			var jsonValue = ele.GetRawText().FromJson<JsonValue>(options: options) ?? throw new JsonException($"'{nameof(JsonValue)}' could not be deserialized from raw text");
			res.InternalDictionary.Add(dis,jsonValue);
			reader.Read();
		}
		return res;
	}
	public override void Write(Utf8JsonWriter writer, JsonPodCollection<TDiscriminator> value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		foreach (var val in value.InternalDictionary.Values)
			val.WriteTo(writer, options);
		writer.WriteEndArray();
	}
}