using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fuxion.Text.Json.Serialization;

namespace Fuxion.Text.Json;

public class JsonNodePod<TDiscriminator>(TDiscriminator discriminator, object payload, IUriKeyResolver? resolver = null) : IPod<TDiscriminator, JsonNode>//, IPod<TDiscriminator, string>
	where TDiscriminator : notnull
{
	// ATTENTION: This constructor cannot be removed, it is needed for deserialization
	protected JsonNodePod() : this(default!, default!) { }
	// ATTENTION: The init setter cannot be removed, it is needed for deserialization
	public TDiscriminator Discriminator { get; init; } = discriminator;
	// ATTENTION: The init setter cannot be removed, it is needed for deserialization
	public JsonNode Payload { get; init; } = CreateValue(payload, resolver);
	static JsonNode CreateValue(object payload, IUriKeyResolver? resolver)
	{
		// TODO ver si podemos mejorar este tratamiento de nullable
		if (payload is null) return null!;
		JsonSerializerOptions options = new();
		options.Converters.Add(new IPodConverterFactory(resolver));
		var node = JsonSerializer.SerializeToNode(payload, options) ?? throw new SerializationException("Serialization returns null");
		return node;
	}
	public T? As<T>()
	{
		JsonSerializerOptions options = new();
		options.Converters.Add(new IPodConverterFactory());
		var res = Payload.Deserialize<T>(options);
		return res ?? default;
	}
	public bool TryAs<T>([NotNullWhen(true)] out T? payload)
	{
		try
		{
			var res = As<T>();
			payload = res;
			return res is not null;
		} catch { }
		payload = default;
		return false;
	}
}