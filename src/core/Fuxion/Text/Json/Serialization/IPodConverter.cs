using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Fuxion.Json;
using Fuxion.Reflection;
using PodType = Fuxion.IPod<string, string>;

namespace Fuxion.Text.Json;

public class IPodConverter<TPod, TDiscriminator, TPayload>(ITypeKeyResolver? resolver = null) : JsonConverter<TPod>
	where TPod : IPod<TDiscriminator, TPayload>
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
		if (type.GetProperty(DISCRIMINATOR_LABEL) is not null) throw new InvalidProgramException($"The pod '{type.Name}' cannot has a property called '{ITEMS_LABEL}', this name is reserved");
		if (type.GetProperty(PAYLOAD_LABEL) is not null) throw new InvalidProgramException($"The pod '{type.Name}' cannot has a property called '{ITEMS_LABEL}', this name is reserved");
		if (type.GetProperty(ITEMS_LABEL) is not null) throw new InvalidProgramException($"The pod '{type.Name}' cannot has a property called '{ITEMS_LABEL}', this name is reserved");
	}
	public override TPod? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		CheckTypeReserverProperties(typeToConvert);
		var ins = Activator.CreateInstance(typeToConvert, true)
			?? throw new InvalidProgramException($"Could not be created instance of type '{typeof(TPod).GetSignature()}' using its private constructor");
		var pod = (TPod)ins;
		if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("The reader expected JsonTokenType.StartObject");
		var jsonObject = JsonObject.Create(JsonDocument.ParseValue(ref reader).RootElement)
			?? throw new SerializationException($"Couldn't be created JsonObject");
		
		// DISCRIMINATOR
		var disNode = jsonObject
			.FirstOrDefault(pair => pair.Key == DISCRIMINATOR_LABEL).Value
			?? throw new SerializationException($"Discriminator couldn't be obtained from label '{DISCRIMINATOR_LABEL}'");
		var disProp = pod.GetType()
				.GetProperty(nameof(IPod<string, string>.Discriminator), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			?? throw new InvalidProgramException($"'{nameof(IPod<string, string>.Discriminator)}' property could not be obtained from pod '{pod.GetType().Name}'");
		var payloadType = typeof(TPayload);
		if (resolver is not null && typeof(TypeKey).IsAssignableFrom(typeof(TDiscriminator)))
		{
			var tk = disNode.Deserialize<TypeKey>(options)
				?? throw new SerializationException($"Couldn't be obtained TypeKey from discriminator");
			payloadType = resolver[tk];
		}
		pod.SetPrivatePropertyValue(disProp.Name, disNode.Deserialize<TDiscriminator>(options));
		
		// PAYLOAD
		var payNode = jsonObject
				.FirstOrDefault(pair => pair.Key == PAYLOAD_LABEL).Value
			?? throw new SerializationException($"Payload couldn't be obtained from label '{PAYLOAD_LABEL}'");
		var payProp = pod.GetType()
				.GetProperty(nameof(IPod<string, string>.Payload), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			?? throw new InvalidProgramException($"'{nameof(IPod<string, string>.Payload)}' property could not be obtained from pod '{pod.GetType().Name}'");
		var payValue = payNode.Deserialize(payloadType, options);
		pod.SetPrivatePropertyValue(payProp.Name, payValue);
		
		// ITEMS
		var iteNode = jsonObject.FirstOrDefault(pair => pair.Key == ITEMS_LABEL).Value;
		if (iteNode is not null)
		{
			if (pod is not ICollectionPod<TDiscriminator, TPayload> col) throw new SerializationException($"{ITEMS_LABEL} is present but pod '{pod.GetType().Name}' is not a collection pod");
			foreach (var node in iteNode.AsArray())
			{
				if (node is not null && resolver is not null)
				{
					var headerPod = (IPod<TDiscriminator, object>?)node.Deserialize(typeof(TypeKeyPod<object>), options)?? throw new SerializationException("Cannot be deserialize header");
					col.Add(headerPod);
				} else
				{
					var headerPod = (JsonNodePod<TDiscriminator>?)node.Deserialize(typeof(JsonNodePod<TDiscriminator>), options) ?? throw new SerializationException("Cannot be deserialize header");
					col.Add(headerPod);
				}
			}
		}
		return pod;
	}
	public override void Write(Utf8JsonWriter writer, TPod value, JsonSerializerOptions options)
	{
		CheckTypeReserverProperties(value.GetType());
		writer.WriteStartObject();
		foreach (var prop in value.GetType()
			.GetProperties()
			.Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null && p.GetIndexParameters()
				.Length == 0)
			.Where(p => p.Name != nameof(PodType.Payload) && p.Name != nameof(PodType.Discriminator)))
		{
			writer.WritePropertyName(prop.GetCustomAttribute<JsonPropertyNameAttribute>()
				?.Name ?? prop.Name);
			writer.WriteRawValue(prop.GetValue(value)
				.SerializeToJson(options: options));
		}
		writer.WritePropertyName(DISCRIMINATOR_LABEL);
		writer.WriteRawValue(value.Discriminator.SerializeToJson(options: options));
		writer.WritePropertyName(PAYLOAD_LABEL);
		if (value.Payload is JsonNode jn)
			jn.WriteTo(writer, options);
		else
			writer.WriteRawValue(value.Payload.SerializeToJson(options: options));
		// Items
		if (value is ICollectionPod<TDiscriminator, TPayload> col)
			if (col.Any())
			{
				writer.WritePropertyName(ITEMS_LABEL);
				writer.WriteStartArray();
				foreach (var val in col) JsonSerializer.Serialize(writer, val, options);
				writer.WriteEndArray();
			}
		writer.WriteEndObject();
	}
}