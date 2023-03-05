using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Fuxion.Json;

[JsonConverter(typeof(JsonPodCollectionConverterFactory))]
public class JsonPodCollection<TDiscriminator>
	where TDiscriminator : notnull
{
	internal Dictionary<TDiscriminator, JsonValue> InternalDictionary { get; } = new();
	[JsonIgnore]
	public int Count => InternalDictionary.Count;
	public void Add<TPayload>(TDiscriminator discriminator, TPayload payload) where TPayload : notnull => Add<TPayload>(new(discriminator, payload));
	public void Add<TPayload>(JsonPod<TDiscriminator, TPayload> pod)
		where TPayload : notnull =>
		InternalDictionary.Add(pod.Discriminator, JsonValue.Create(pod) ?? throw new ArgumentException($"The pod of type '{pod.GetType().GetSignature()}' cannot be serialized"));
	public bool Has(TDiscriminator discriminator) => InternalDictionary.ContainsKey(discriminator);
	public JsonPod<TDiscriminator, TPayload>? Get<TPayload>(TDiscriminator discriminator)
		where TPayload : notnull =>
		InternalDictionary[discriminator].Deserialize<JsonPod<TDiscriminator, TPayload>>();
}