using System.Collections;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using PodType = Fuxion.IPod2<string,string>;

namespace Fuxion.Json;

public class JsonPod2Converter<TPod, TDiscriminator, TPayload> : JsonConverter<TPod> 
	where TPod : JsonPod2<TDiscriminator, TPayload>
	where TDiscriminator : notnull
{
	public const string ITEMS_LABEL = "__items";
	public const string PAYLOAD_LABEL = "__payload";
	public const string DISCRIMINATOR_LABEL = "__discriminator";
	PropertyInfo? SearchProperty(object target, string propertyName)
	{
		var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		var property = target.GetType()
			.GetProperties(flags)
			.Where(p => p.GetCustomAttribute<JsonPropertyNameAttribute>() != null)
			.SingleOrDefault(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()
				?.Name == propertyName);
		if (property is not null) return property;
		property = target.GetType()
			.GetProperty(propertyName, flags);
		if (property?.HasCustomAttribute<JsonPropertyNameAttribute>() ?? false) return null;
		return property;
	}
	public override TPod? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var ins = Activator.CreateInstance(typeof(TPod), true)
			?? throw new InvalidProgramException($"Could not be created instance of type '{typeof(TPod).GetSignature()}' using its private constructor"); 
		var pod = (TPod)ins;
		if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("The reader expected JsonTokenType.StartObject");
		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject) return pod;
			if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException($"The reader expected '{JsonTokenType.PropertyName}', but is '{reader.TokenType}'");
			var propertyName = reader.GetString() ?? throw new InvalidProgramException("Current property name could not be read from Utf8JsonReader.");
			//var prop = pod.GetType().GetProperty(propertyName);// ?? throw new InvalidProgramException($"Property '{propertyName}' does not exist in type '{typeof(TPod).GetSignature()}'");
			var prop = SearchProperty(pod, propertyName);
			var ele = JsonDocument.ParseValue(ref reader).RootElement;
			if (prop == null)
			{
				if (propertyName == ITEMS_LABEL)
				{
					foreach (var ele2 in ele.EnumerateArray())
					{
						var pro = ele2.GetProperty(DISCRIMINATOR_LABEL);
						var dis = (TDiscriminator)(pro.Deserialize(typeof(TDiscriminator), options) ?? throw new JsonException($"Property '{nameof(JsonPod2<int, int>.Discriminator)}' with kind '{pro.ValueKind}' could not be deserialized"));
						var jsonValue = ele2.GetRawText().DeserializeFromJson<JsonValue>(options: options) ?? throw new JsonException($"'{nameof(JsonValue)}' could not be deserialized from raw text");
						pod.InternalDictionary.Add(dis,jsonValue);
					}
				}
				continue;
			}
			if (propertyName == PAYLOAD_LABEL)
			{
				var rawProp = pod.GetType().GetProperty(nameof(JsonPod2<string, string>.PayloadValue), BindingFlags.NonPublic | BindingFlags.Instance)
					?? throw new InvalidProgramException($"'{nameof(JsonPod2<string, string>.PayloadValue)}' property could not be obtained from pod object");
				var jsonValue = ele.GetRawText().DeserializeFromJson<JsonValue>(options: options);
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
		if(value.GetType().GetProperty(ITEMS_LABEL) is not null)
			throw new InvalidProgramException($"The pod '{value.GetType().Name}' cannot has a property called '{ITEMS_LABEL}', this name is reserved");
		writer.WriteStartObject();
		foreach (var prop in value.GetType().GetProperties().Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null && p.GetIndexParameters().Length == 0))
		{
			writer.WritePropertyName(prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name);
			writer.WriteRawValue(prop.GetValue(value)
				.SerializeToJson(options: options));
		}
		var rawProp = value.GetType().GetProperty(nameof(JsonPod2<string, string>.PayloadValue), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
			?? throw new InvalidProgramException($"The pod '{value.GetType().Name}' doesn't has '{nameof(JsonPod2<string, string>.PayloadValue)}' property");
		writer.WritePropertyName(rawProp.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? rawProp.Name);
		if(value.PayloadValue is null)
			writer.WriteNullValue();
		else
			value.PayloadValue.WriteTo(writer, options);
		
		// Items
		if (value.Count > 0)
		{
			writer.WritePropertyName(ITEMS_LABEL);
			writer.WriteStartArray();
			foreach (var val in value.InternalDictionary.Values)
				val.WriteTo(writer, options);
			writer.WriteEndArray();
		}
		
		writer.WriteEndObject();
	}
}
public class IPod2Converter<TPod, TDiscriminator, TPayload> : JsonConverter<TPod> 
	where TPod : IPod2<TDiscriminator, TPayload>
	where TDiscriminator : notnull
{
	public const string ITEMS_LABEL = "__items";
	public const string PAYLOAD_LABEL = "__payload";
	public const string DISCRIMINATOR_LABEL = "__discriminator";
	PropertyInfo? SearchProperty(object target, string propertyName)
	{
		var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		var property = target.GetType()
			.GetProperties(flags)
			.Where(p => p.GetCustomAttribute<JsonPropertyNameAttribute>() != null)
			.SingleOrDefault(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()
				?.Name == propertyName);
		if (property is not null) return property;
		property = target.GetType()
			.GetProperty(propertyName, flags);
		if (property?.HasCustomAttribute<JsonPropertyNameAttribute>() ?? false) return null;
		return property;
	}
	void CheckTypeReserverProperties(Type type)
	{
		if(type.GetProperty(DISCRIMINATOR_LABEL) is not null)
			throw new InvalidProgramException($"The pod '{type.Name}' cannot has a property called '{ITEMS_LABEL}', this name is reserved");
		if(type.GetProperty(PAYLOAD_LABEL) is not null)
			throw new InvalidProgramException($"The pod '{type.Name}' cannot has a property called '{ITEMS_LABEL}', this name is reserved");
		if(type.GetProperty(ITEMS_LABEL) is not null)
			throw new InvalidProgramException($"The pod '{type.Name}' cannot has a property called '{ITEMS_LABEL}', this name is reserved");
	}
	public override TPod? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		CheckTypeReserverProperties(typeToConvert);
		var ins = Activator.CreateInstance(typeToConvert, true)
			?? throw new InvalidProgramException($"Could not be created instance of type '{typeof(TPod).GetSignature()}' using its private constructor"); 
		var pod = (TPod)ins;
		if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("The reader expected JsonTokenType.StartObject");
		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject) return pod;
			if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException($"The reader expected '{JsonTokenType.PropertyName}', but is '{reader.TokenType}'");
			var propertyName = reader.GetString() ?? throw new InvalidProgramException("Current property name could not be read from Utf8JsonReader.");
			var prop = SearchProperty(pod, propertyName);
			var ele = JsonDocument.ParseValue(ref reader).RootElement;
			if (prop == null)
			{
				switch (propertyName)
				{
					case DISCRIMINATOR_LABEL:
					{
						var disProp = pod.GetType().GetProperty(nameof(IPod2<string, string>.Discriminator), BindingFlags.Public |BindingFlags.NonPublic | BindingFlags.Instance)
							?? throw new InvalidProgramException($"'{nameof(IPod2<string, string>.Discriminator)}' property could not be obtained from pod '{pod.GetType().Name}'");
						var disValue = ele.GetRawText().DeserializeFromJson<TDiscriminator>(options: options);
						// disProp.SetPrivatePropertyValue(pod, disValue);
						pod.SetPrivatePropertyValue(disProp.Name, disValue);
						// disProp.SetValue(pod, disValue);
						break;
					}
					case PAYLOAD_LABEL:
					{
						var payProp = pod.GetType().GetProperty(nameof(IPod2<string, string>.Payload), BindingFlags.Public |BindingFlags.NonPublic | BindingFlags.Instance)
							?? throw new InvalidProgramException($"'{nameof(IPod2<string, string>.Payload)}' property could not be obtained from pod '{pod.GetType().Name}'");
						var payValue = ele.GetRawText().DeserializeFromJson<TPayload>(options: options);
						pod.SetPrivatePropertyValue(payProp.Name, payValue);
						// payProp.SetValue(pod, payValue);
						// try
						// {
						// 	var val = ele.Deserialize(prop.PropertyType, new JsonSerializerOptions {
						// 		TypeInfoResolver = new PrivateConstructorContractResolver()
						// 	});
						// 	pod.SetPrivatePropertyValue(prop.Name, val);
						// } catch { }
						break;
					}
					case ITEMS_LABEL:
					{
						if (pod is not ICollectionPod2<TDiscriminator, TPayload> col)
							throw new SerializationException($"{ITEMS_LABEL} is present but pod '{pod.GetType().Name}' is not a collection pod");
						foreach (var ele2 in ele.EnumerateArray())
						{
							var headerPod = (JsonNodePod2<TDiscriminator>?)ele2.Deserialize(typeof(JsonNodePod2<TDiscriminator>), options)
								?? throw new SerializationException($"Cannot be deserialize header");
							col.Add(headerPod);
						
							// 		var pro = ele2.GetProperty(DISCRIMINATOR_LABEL);
							// var dis = (TDiscriminator)(pro.Deserialize(typeof(TDiscriminator), options) ?? throw new JsonException($"Property '{nameof(JsonPod2<int, int>.Discriminator)}' with kind '{pro.ValueKind}' could not be deserialized"));
							// var jsonValue = ele2.GetRawText().FromJson<JsonValue>(options: options) ?? throw new JsonException($"'{nameof(JsonValue)}' could not be deserialized from raw text");
							// col.InternalDictionary.Add(dis,jsonValue);
						}
						break;
					}
				}
			} else
			{
				var val = ele.Deserialize(prop.PropertyType, new JsonSerializerOptions {
					TypeInfoResolver = new PrivateConstructorContractResolver()
				});
				pod.SetPrivatePropertyValue(prop.Name, val);
			}
			// if (prop == null)
			// {
			// 	if (propertyName == ITEMS_LABEL && pod is ICollectionPod2<TDiscriminator, TPayload> col)
			// 	{
			// 		foreach (var ele2 in ele.EnumerateArray())
			// 		{
			// 			var pro = ele2.GetProperty(DISCRIMINATOR_LABEL);
			// 			var dis = (TDiscriminator)(pro.Deserialize(typeof(TDiscriminator), options) ?? throw new JsonException($"Property '{nameof(JsonPod2<int, int>.Discriminator)}' with kind '{pro.ValueKind}' could not be deserialized"));
			// 			var jsonValue = ele2.GetRawText().FromJson<JsonValue>(options: options) ?? throw new JsonException($"'{nameof(JsonValue)}' could not be deserialized from raw text");
			// 			// col.InternalDictionary.Add(dis,jsonValue);
			// 		}
			// 	}
			// 	continue;
			// }
			// if (propertyName == PAYLOAD_LABEL)
			// {
			// 	var rawProp = pod.GetType().GetProperty(nameof(JsonPod2<string, string>.PayloadValue), BindingFlags.NonPublic | BindingFlags.Instance)
			// 		?? throw new InvalidProgramException($"'{nameof(JsonPod2<string, string>.PayloadValue)}' property could not be obtained from pod object");
			// 	var jsonValue = ele.GetRawText().FromJson<JsonValue>(options: options);
			// 	rawProp.SetValue(pod, jsonValue);
			// 	try
			// 	{
			// 		var val = ele.Deserialize(prop.PropertyType, new JsonSerializerOptions {
			// 			TypeInfoResolver = new PrivateConstructorContractResolver()
			// 		});
			// 		pod.SetPrivatePropertyValue(prop.Name, val);
			// 	} catch { }
			// } else
			// {
			// 	var val = ele.Deserialize(prop.PropertyType, new JsonSerializerOptions {
			// 		TypeInfoResolver = new PrivateConstructorContractResolver()
			// 	});
			// 	pod.SetPrivatePropertyValue(prop.Name, val);
			// }
		}
		return pod;
	}
	public override void Write(Utf8JsonWriter writer, TPod value, JsonSerializerOptions options)
	{
		CheckTypeReserverProperties(value.GetType());
		writer.WriteStartObject();
		foreach (var prop in value.GetType().GetProperties()
			.Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null && p.GetIndexParameters().Length == 0)
			.Where(p => p.Name != nameof(PodType.Payload) && p.Name != nameof(PodType.Discriminator)))
		{
			writer.WritePropertyName(prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name);
			writer.WriteRawValue(prop.GetValue(value).SerializeToJson(options:options));
		}
		// var rawProp = value.GetType().GetProperty(nameof(IPod2<string, string>.Payload), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
		// 	?? throw new InvalidProgramException($"The pod '{value.GetType().Name}' doesn't has '{nameof(JsonPod2<string, string>.PayloadValue)}' property");
		// writer.WritePropertyName(rawProp.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? rawProp.Name);
		writer.WritePropertyName(DISCRIMINATOR_LABEL);
		writer.WriteRawValue(value.Discriminator.SerializeToJson(options:options));
		writer.WritePropertyName(PAYLOAD_LABEL);
		if (value.Payload is JsonNode jn)
			jn.WriteTo(writer, options);
		else
			writer.WriteRawValue(value.Payload.SerializeToJson(options:options));
		
		// if(value.Payload is null)
		// 	writer.WriteNullValue();
		// else
		// 	value.Payload.WriteTo(writer, options);
		
		// Items
		if (value is ICollectionPod2<TDiscriminator, TPayload> col)
		{
			if (col.Any())
			{
				writer.WritePropertyName(ITEMS_LABEL);
				writer.WriteStartArray();
				foreach (var val in col) JsonSerializer.Serialize(writer, val, options);
				writer.WriteEndArray();
			}
		}
		// if (value.GetType().IsSubclassOfRawGeneric(typeof(ICollectionPod2<,>)))
		// {
		// 	var col = (IEnumerable)value;
		// 	if(col.)
		// }
		// if (value.Count > 0)
		// {
		// 	writer.WritePropertyName(ITEMS_LABEL);
		// 	writer.WriteStartArray();
		// 	foreach (var val in value.InternalDictionary.Values)
		// 		val.WriteTo(writer, options);
		// 	writer.WriteEndArray();
		// }
		
		writer.WriteEndObject();
	}
}