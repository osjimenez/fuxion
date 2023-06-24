// using System.Collections;
// using System.Diagnostics.CodeAnalysis;
// using System.Text.Json;
// using System.Text.Json.Nodes;
// using System.Text.Json.Serialization;
//
// namespace Fuxion.Json;
//
// [JsonConverter(typeof(JsonPodCollection2ConverterFactory))]
// public class JsonPodCollection2<TDiscriminator> : IPodCollection2<TDiscriminator>
// 	where TDiscriminator : notnull
// {
// 	internal Dictionary<TDiscriminator, JsonValue> InternalDictionary { get; } = new();
// 	[JsonIgnore]
// 	public int Count => InternalDictionary.Count;
// 	public bool Has(TDiscriminator discriminator) => InternalDictionary.ContainsKey(discriminator);
// 	public IPod2<TDiscriminator, object> this[TDiscriminator discriminator] =>
// 		InternalDictionary[discriminator].ToString().FromJson<JsonPod2<TDiscriminator, object>>() 
// 		?? throw new ArgumentException($"'{nameof(JsonValue)}' for discriminator '{discriminator}' couldn't be deserialized as '{typeof(JsonPod2<TDiscriminator, object>).GetSignature()}'");
// 	public void Add(IPod2<TDiscriminator, object> pod)
// 	{
// 		if(pod.GetType().IsSubclassOfRawGeneric(typeof(JsonPod2<,>)))
// 			InternalDictionary.Add(pod.Discriminator, new JsonPod2<TDiscriminator, object>(pod.Discriminator, pod).PayloadValue);
// 		else throw new ArgumentException($"In '{this.GetType().GetSignature()}' only '{typeof(JsonPod2<TDiscriminator, object>).GetSignature()}' can be added");
// 	}
// 	public bool Remove(TDiscriminator discriminator) => InternalDictionary.Remove(discriminator);
// 	public IEnumerator<IPod2<TDiscriminator, object>> GetEnumerator() =>
// 		InternalDictionary.Select(_ => new JsonPod2<TDiscriminator, object>(_.Key, _.Value)).GetEnumerator();
// }