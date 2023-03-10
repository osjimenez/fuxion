using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Fuxion.Json;

// public class JsonPodConverter<TPod, TDiscriminator, TPayload> : JsonPodConverter<TPod, TDiscriminator, TPayload, TDiscriminator>
// 	where TPod : JsonPod<TDiscriminator, TPayload, TDiscriminator>
// 	// where TPayload : notnull
// 	where TDiscriminator : notnull 
// { }

public class JsonPodConverter<TPod, TDiscriminator, TPayload> : JsonConverter<TPod> 
	// where TCollection : JsonPodCollection<TDiscriminator>
	where TPod : JsonPod<TDiscriminator, TPayload>
	// where TPayload : notnull
	where TDiscriminator : notnull
{
	public override TPod? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var ins = Activator.CreateInstance(typeof(TPod), true)
			?? throw new InvalidProgramException($"Could not be created instance of type '{typeof(TPod).GetSignature()}' using its private constructor"); 
		var pod = (TPod)ins;
		if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();
		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject) return pod;
			if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("The reader expected JsonTokenType.PropertyName");
			var propertyName = reader.GetString() ?? throw new InvalidProgramException("Current property name could not be read from Utf8JsonReader.");
			var prop = pod.GetType().GetProperty(propertyName);// ?? throw new InvalidProgramException($"Property '{propertyName}' does not exist in type '{typeof(TPod).GetSignature()}'");
			if (prop == null)
			{
				reader.Read();
				continue;
			}
			var ele = JsonDocument.ParseValue(ref reader).RootElement;
			if (propertyName == nameof(JsonPod<string, string>.Payload))
			{
				var rawProp = pod.GetType().GetProperty(nameof(JsonPod<string, string>.PayloadValue), BindingFlags.NonPublic | BindingFlags.Instance)
					?? throw new InvalidProgramException($"'{nameof(JsonPod<string, string>.PayloadValue)}' property could not be obtained from pod object");
				var jsonValue = ele.GetRawText().FromJson<JsonValue>(options: options);
				rawProp.SetValue(pod, jsonValue);
				try
				{
					var val = ele.Deserialize(prop.PropertyType, new JsonSerializerOptions {
						TypeInfoResolver = new PrivateConstructorContractResolver()
					});
					pod.SetPrivatePropertyValue(prop.Name, val);
				} catch { }
			} else
			{
				var val = ele.Deserialize(prop.PropertyType, new JsonSerializerOptions {
					TypeInfoResolver = new PrivateConstructorContractResolver()
				});
				pod.SetPrivatePropertyValue(prop.Name, val);
			}
		}
		return pod;
	}
	public override void Write(Utf8JsonWriter writer, TPod value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		foreach (var prop in value.GetType().GetProperties().Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null))
		{
			if (prop.Name == nameof(JsonPod<string, string>.Headers))
			{
				if (prop.GetValue(value) is not JsonPodCollection<TDiscriminator> headers)
					throw new InvalidProgramException($"Property '{prop.Name}' of type '{prop.DeclaringType?.Name}' must be of Type '{nameof(JsonPodCollection<TDiscriminator>)}'");
				if (headers.Count <= 0) continue;
			}
			writer.WritePropertyName(prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name);
			writer.WriteRawValue(prop.GetValue(value).ToJson(options));
		}
		var rawProp = value.GetType().GetProperty(nameof(JsonPod<string, string>.PayloadValue), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
			?? throw new InvalidProgramException($"The pod '{value.GetType().Name}' doesn't has '{nameof(JsonPod<string, string>.PayloadValue)}' property");
		writer.WritePropertyName(rawProp.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? rawProp.Name);
		value.PayloadValue.WriteTo(writer, options);
		writer.WriteEndObject();
	}
}