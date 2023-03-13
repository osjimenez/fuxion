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
		new JsonPod<TDiscriminator, object>(discriminator, InternalDictionary[discriminator]);
	public void Add(IPod<TDiscriminator, object, object> pod) => 
		InternalDictionary.Add(pod.Discriminator, new JsonPod<TDiscriminator, object>(pod.Discriminator, pod.Outside()).PayloadValue);
	public void Remove(TDiscriminator discriminator) => InternalDictionary.Remove(discriminator);
	public IEnumerator<IPod<TDiscriminator, object, JsonValue, JsonPodCollection<TDiscriminator>>> GetEnumerator() =>
		InternalDictionary.Select(_ => new JsonPod<TDiscriminator, object>(_.Key, _.Value)).GetEnumerator();
	//
	// void IPodCollection<TDiscriminator, IPod<TDiscriminator, object, JsonValue, JsonPodCollection<TDiscriminator>>>.Add<TOutside, TInside>(TDiscriminator discriminator, TInside inside) =>
	// 	podCollectionImplementation.Add<TOutside, TInside>(discriminator, inside);
	// void IPodCollection<TDiscriminator, IPod<TDiscriminator, object, JsonValue, JsonPodCollection<TDiscriminator>>>.Add<TOutside, TInside>(TDiscriminator discriminator, TOutside outside) =>
	// 	podCollectionImplementation.Add<TOutside, TInside>(discriminator, outside);
	//
	// void Add<TOutside, TInside>(TDiscriminator discriminator, TOutside outside) => Add(new JsonPod<TDiscriminator, TOutside>(discriminator, outside));
	//
	//
	//
	// public void AddByOutside<TPayload>(TDiscriminator discriminator, TPayload payload) //where TPayload : notnull 
	// 	=> Add(new JsonPod<TDiscriminator, TPayload>(discriminator, payload));
	// void IPodCollection<TDiscriminator, IPod<TDiscriminator, object, JsonValue, JsonPodCollection<TDiscriminator>>>.AddByOutside<TPayload>(IPod<TDiscriminator, object, JsonValue, JsonPodCollection<TDiscriminator>> pod)
	// {
	// 	if (pod is JsonPod<TDiscriminator, TPayload> jsonPod)
	// 		Add(jsonPod);
	// 	else
	// 		throw new ArgumentException($"'{GetType().GetSignature()}' only allow '{typeof(JsonPod<TDiscriminator, TPayload>).GetSignature()}' items");
	// }
	// public void Add<TPayload>(JsonPod<TDiscriminator, TPayload> pod)
	// 	=> InternalDictionary.Add(pod.Discriminator, JsonValue.Create(pod) ?? throw new ArgumentException($"The pod of type '{pod.GetType().GetSignature()}' cannot be serialized"));
	//
	//
	// public void Replace<TPayload>(TDiscriminator discriminator, TPayload payload) //where TPayload : notnull 
	// 	=> Replace<TPayload>(new(discriminator, payload));
	// public void Replace<TPayload>(JsonPod<TDiscriminator, TPayload> pod) //where TPayload : notnull
	// {
	// 	if (!InternalDictionary.ContainsKey(pod.Discriminator)) throw new KeyNotFoundException($"No header with discriminator '{pod.Discriminator}' was found to replace");
	// 	InternalDictionary[pod.Discriminator] = JsonValue.Create(pod) ?? throw new ArgumentException($"The pod of type '{pod.GetType().GetSignature()}' cannot be serialized");
	// }
	//
	//
	// public bool Has(TDiscriminator discriminator) => InternalDictionary.ContainsKey(discriminator);
	//
	//
	// IPod<TDiscriminator, object, JsonValue, JsonPodCollection<TDiscriminator>>? IPodCollection<TDiscriminator, IPod<TDiscriminator,object, JsonValue, JsonPodCollection<TDiscriminator>>>.GetPod<TPayload>(TDiscriminator discriminator)
	// 	=> GetPod<TPayload>(discriminator)?.CastWithPayload<object>();
	// public JsonPod<TDiscriminator, TPayload>? GetPod<TPayload>(TDiscriminator discriminator)
	// 	//where TPayload : notnull
	// 	=> InternalDictionary[discriminator].Deserialize<JsonPod<TDiscriminator, TPayload>>();
	// bool IPodCollection<TDiscriminator, IPod<TDiscriminator, object, JsonValue, JsonPodCollection<TDiscriminator>>>.TryGetPod<TPayload>(
	// 	TDiscriminator discriminator, [NotNullWhen(true)] out IPod<TDiscriminator, object, JsonValue, JsonPodCollection<TDiscriminator>>? pod)
	// {
	// 	TryGetPod<TPayload>(discriminator, out var res);
	// 	pod = res?.CastWithPayload<object>();
	// 	return res is not null;
	// }
	// public bool TryGetPod<TPayload>(TDiscriminator discriminator, out JsonPod<TDiscriminator, TPayload>? pod)
	// {
	// 	var res = GetPod<TPayload>(discriminator);
	// 	pod = res;
	// 	return res is not null;
	// }
	//
	//
	// public TPayload? GetPayload<TPayload>(TDiscriminator discriminator)
	// {
	// 	var res = InternalDictionary[discriminator].Deserialize<JsonPod<TDiscriminator, TPayload>>();
	// 	return res is null ? default : res.Payload;
	// }
	// public bool TryGetPayload<TPayload>(TDiscriminator discriminator, [NotNullWhen(true)] out TPayload? payload)
	// {
	// 	var res = InternalDictionary[discriminator].Deserialize<TPayload>();
	// 	payload = res;
	// 	return res is not null;
	// }
	// IPod<TDiscriminator, object, JsonValue, JsonPodCollection<TDiscriminator>> IPodCollection<TDiscriminator, IPod<TDiscriminator, object, JsonValue, JsonPodCollection<TDiscriminator>>>.this[TDiscriminator discriminator] =>
	// 	GetPod<object>(discriminator) ?? throw new ArgumentException($"Discriminator '{discriminator}' not found");
	//
	// public IEnumerator<IPod<TDiscriminator,object, JsonValue, JsonPodCollection<TDiscriminator>>> GetEnumerator() 
	// 	=> InternalDictionary.Select(_ => _.Value.Deserialize<IPod<TDiscriminator,object,JsonValue,JsonPodCollection<TDiscriminator>>>()!).GetEnumerator();

}