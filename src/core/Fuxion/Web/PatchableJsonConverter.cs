using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fuxion.Json;

namespace Fuxion.Web;

public class PatchableJsonConverter<T> : JsonConverter<Patchable<T>> where T : class
{
	public override Patchable<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var ins = Activator.CreateInstance(typeof(Patchable<T>), true);
		if (ins is null) throw new InvalidProgramException($"Could not be created instance of type '{typeof(Patchable<T>).GetSignature()}' using its private constructor");
		var patchable = (Patchable<T>)ins;
		if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();
		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject) return patchable;
			if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("The reader expected JsonTokenType.PropertyName");
			var propertyName = reader.GetString() ?? throw new InvalidProgramException("Current property name could not be read from Utf8JsonReader.");
			var prop = typeof(T).GetRuntimeProperty(propertyName);
			if (prop is null) throw new InvalidProgramException($"The property '{propertyName}' is not present in type '{typeof(T)}'");
			var ele = JsonDocument.ParseValue(ref reader).RootElement;
			var val = ele.Deserialize(prop.PropertyType, new JsonSerializerOptions {
				TypeInfoResolver = new PrivateConstructorContractResolver()
			});
			patchable.Properties.Add(propertyName, (prop, val));
		}
		return patchable;
	}
	public override void Write(Utf8JsonWriter writer, Patchable<T> value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		foreach (var pvk in value.Properties)
		{
			writer.WritePropertyName(pvk.Key);
			writer.WriteRawValue(pvk.Value.Value.SerializeToJson(options:options));
		}
		writer.WriteEndObject();
	}
}