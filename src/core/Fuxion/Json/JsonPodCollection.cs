using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Fuxion.Json;

[JsonConverter(typeof(JsonPodCollectionConverterFactory))]
public class JsonPodCollection<TDiscriminator> : IPodCollection<TDiscriminator, IPod<TDiscriminator,object, JsonValue, JsonPodCollection<TDiscriminator>>>// IEnumerable<(TDiscriminator, JsonValue)> 
	where TDiscriminator : notnull
{
	internal Dictionary<TDiscriminator, JsonValue> InternalDictionary { get; } = new();
	[JsonIgnore]
	public int Count => InternalDictionary.Count;
	public bool Has(TDiscriminator discriminator) => InternalDictionary.ContainsKey(discriminator);
	public IPod<TDiscriminator, object, JsonValue, JsonPodCollection<TDiscriminator>> this[TDiscriminator discriminator] =>
		InternalDictionary[discriminator].ToString().FromJson<JsonPod<TDiscriminator, object>>() ?? throw new ArgumentException($"oooooooooooooooooo");
	public void Add(IPod<TDiscriminator, object, object> pod) => 
		InternalDictionary.Add(pod.Discriminator, new JsonPod<TDiscriminator, object>(pod.Discriminator, pod).PayloadValue);
	public void Remove(TDiscriminator discriminator) => InternalDictionary.Remove(discriminator);
	public IEnumerator<IPod<TDiscriminator, object, JsonValue, JsonPodCollection<TDiscriminator>>> GetEnumerator() =>
		InternalDictionary.Select(_ => new JsonPod<TDiscriminator, object>(_.Key, _.Value)).GetEnumerator();
}