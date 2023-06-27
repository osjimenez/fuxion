using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Fuxion.Json;
using PodType = Fuxion.IPod<string, string>;

namespace Fuxion.Text.Json;

public class IPodConverter<TPod, TDiscriminator, TPayload> : JsonConverter<TPod>
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
		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject) return pod;
			if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException($"The reader expected '{JsonTokenType.PropertyName}', but is '{reader.TokenType}'");
			var propertyName = reader.GetString() ?? throw new InvalidProgramException("Current property name could not be read from Utf8JsonReader.");
			var prop = SearchProperty(pod, propertyName);
			var ele = JsonDocument.ParseValue(ref reader)
				.RootElement;
			if (prop == null)
				switch (propertyName)
				{
					case DISCRIMINATOR_LABEL:
					{
						var disProp = pod.GetType()
								.GetProperty(nameof(IPod<string, string>.Discriminator), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
							?? throw new InvalidProgramException($"'{nameof(IPod<string, string>.Discriminator)}' property could not be obtained from pod '{pod.GetType().Name}'");
						var disValue = ele.GetRawText()
							.DeserializeFromJson<TDiscriminator>(options: options);
						pod.SetPrivatePropertyValue(disProp.Name, disValue);
						break;
					}
					case PAYLOAD_LABEL:
					{
						var payProp = pod.GetType()
								.GetProperty(nameof(IPod<string, string>.Payload), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
							?? throw new InvalidProgramException($"'{nameof(IPod<string, string>.Payload)}' property could not be obtained from pod '{pod.GetType().Name}'");
						var payValue = ele.GetRawText()
							.DeserializeFromJson<TPayload>(options: options);
						pod.SetPrivatePropertyValue(payProp.Name, payValue);
						break;
					}
					case ITEMS_LABEL:
					{
						if (pod is not ICollectionPod<TDiscriminator, TPayload> col) throw new SerializationException($"{ITEMS_LABEL} is present but pod '{pod.GetType().Name}' is not a collection pod");
						foreach (var ele2 in ele.EnumerateArray())
						{
							var headerPod = (JsonNodePod<TDiscriminator>?)ele2.Deserialize(typeof(JsonNodePod<TDiscriminator>), options) ?? throw new SerializationException("Cannot be deserialize header");
							col.Add(headerPod);
						}
						break;
					}
				}
			else
			{
				var val = ele.Deserialize(prop.PropertyType, new JsonSerializerOptions
				{
					TypeInfoResolver = new PrivateConstructorContractResolver()
				});
				pod.SetPrivatePropertyValue(prop.Name, val);
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