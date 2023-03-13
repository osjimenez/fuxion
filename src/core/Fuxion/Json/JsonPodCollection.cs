using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Fuxion.Json;

[JsonConverter(typeof(JsonPodCollectionConverterFactory))]
public class JsonPodCollection<TDiscriminator> : IPodCollection<TDiscriminator, JsonPod<TDiscriminator, object>>
	where TDiscriminator : notnull
{
	internal Dictionary<TDiscriminator, JsonValue> InternalDictionary { get; } = new();
	[JsonIgnore]
	public int Count => InternalDictionary.Count;
	public bool Has(TDiscriminator discriminator) => InternalDictionary.ContainsKey(discriminator);
	public JsonPod<TDiscriminator, object> this[TDiscriminator discriminator] =>
		InternalDictionary[discriminator].ToString().FromJson<JsonPod<TDiscriminator, object>>() 
		?? throw new ArgumentException($"'{nameof(JsonValue)}' for discriminator '{discriminator}' couldn't be deserialized as '{typeof(JsonPod<TDiscriminator, object>).GetSignature()}'");
	public void Add(IPod<TDiscriminator, object, object> pod)
	{
		if(pod.GetType().IsSubclassOfRawGeneric(typeof(JsonPod<,>)))
			InternalDictionary.Add(pod.Discriminator, new JsonPod<TDiscriminator, object>(pod.Discriminator, pod).PayloadValue);
		else throw new ArgumentException($"In '{this.GetType().GetSignature()}' only '{typeof(JsonPod<TDiscriminator, object>).GetSignature()}' can be added");
	}
	public bool Remove(TDiscriminator discriminator) => InternalDictionary.Remove(discriminator);
	public IEnumerator<JsonPod<TDiscriminator, object>> GetEnumerator() =>
		InternalDictionary.Select(_ => new JsonPod<TDiscriminator, object>(_.Key, _.Value)).GetEnumerator();
}