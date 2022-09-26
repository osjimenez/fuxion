namespace Fuxion.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

public class JsonPodConverter<TPod, TPayload, TKey> : JsonConverter<TPod> where TPod : JsonPod<TPayload, TKey>
{
	//public override bool CanConvert(Type typeToConvert)
	//	=> base.CanConvert(typeToConvert)
	//		|| typeToConvert.IsSubclassOfRawGeneric(typeof(JsonPod<,>));
	public override TPod? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var pod = Activator.CreateInstance<TPod>();
		if (reader.TokenType != JsonTokenType.StartObject)
		{
			throw new JsonException();
		}
		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
			{
				return pod;
			}
			// Get the key.
			if (reader.TokenType != JsonTokenType.PropertyName)
			{
				throw new JsonException();
			}



			string propertyName = reader.GetString() ?? throw new InvalidProgramException("");
			PropertyInfo prop = pod.GetType().GetProperty(propertyName) ?? throw new InvalidProgramException("");
			prop.SetValue(pod, JsonDocument.ParseValue(ref reader).RootElement.Clone());

			//var converterType = typeof(JsonConverter<>).MakeGenericType(prop.PropertyType);
			//var converter = options.GetConverter(converterType);
			//var args = new object[] { reader };
			//converterType.GetMethod("Read")?.Invoke(converter, new object { ref reader, prop.PropertyType, options });
		}

		//var t = reader.TokenType;
		//Printer.WriteLine(reader.TokenType.ToString());
		//throw new NotImplementedException();
		return pod;
	}
	public override void Write(Utf8JsonWriter writer, TPod value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		foreach (var prop in value.GetType().GetProperties()
			.Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null))
		{
			writer.WritePropertyName(prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name);
			writer.WriteRawValue(JsonSerializer.Serialize(prop.GetValue(value), options));
		}
		var rawProp = value.GetType().GetProperty(nameof(JsonPod.PayloadRaw), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		if (rawProp is null) throw new InvalidProgramException($"The pod '{value.GetType().Name}' doesn't has '{nameof(JsonPod.PayloadRaw)}' property");
		writer.WritePropertyName(rawProp.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? rawProp.Name);
		writer.WriteRawValue(value.PayloadRaw);
		writer.WriteEndObject();
	}
}

public class JsonPodEmptyConverter : JsonConverter<JsonPod>
{
	public override bool CanConvert(Type typeToConvert)
	=> base.CanConvert(typeToConvert)
			   || typeToConvert.IsSubclassOfRawGeneric(typeof(JsonPod<,>));
	public override JsonPod? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> throw new NotImplementedException($"You must use 'JsonPodConverter<TPayload, TKey>' when deserialize '{nameof(JsonPod)}<>' objects");
	public override void Write(Utf8JsonWriter writer, JsonPod value, JsonSerializerOptions options)
		=> throw new NotImplementedException($"You must use 'JsonPodConverter<TPayload, TKey>' when serialize '{nameof(JsonPod)}<>' objects");
}
