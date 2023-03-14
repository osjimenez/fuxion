using System.ComponentModel.DataAnnotations;
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
		List<string> keyChain = new();
		if (Uri.TryCreate(discriminator, UriKind.Absolute, out var uri))
		{
			if (!string.IsNullOrWhiteSpace(uri.Query)) throw new TypeKeyException($"The {nameof(TypeKey)} couldn't be deserialized from '{discriminator}', the query '{uri.Query}' in the url is not supported");
			keyChain.Add($"{uri.Scheme}://{uri.Host}");
			keyChain.AddRange(uri.PathAndQuery.Trim('/').Split('/'));
		}else keyChain.AddRange(discriminator.Split('/'));
		return new(keyChain.ToArray());
	}
	public override void Write(Utf8JsonWriter writer, TypeKey value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}