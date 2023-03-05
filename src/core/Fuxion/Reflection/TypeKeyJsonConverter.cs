using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fuxion.Reflection;

public class TypeKeyJsonConverter : JsonConverter<TypeKey>{
	public override TypeKey? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if(reader.TokenType != JsonTokenType.String)
			throw new JsonException($"TypeKey deserialization: reader must be in '{JsonTokenType.String}' token but in '{reader.TokenType}'");
		var discriminator = reader.GetString();
		if(string.IsNullOrWhiteSpace(discriminator)) throw new JsonException($"TypeKey deserialization: value cannot be null or empty string");
		return new(discriminator.Split('/'));
	}
	public override void Write(Utf8JsonWriter writer, TypeKey value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}