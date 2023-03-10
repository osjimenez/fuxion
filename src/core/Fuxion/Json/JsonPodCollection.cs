using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Fuxion.Json;

[JsonConverter(typeof(JsonPodCollectionConverterFactory))]
public class JsonPodCollection<TDiscriminator> : IPodCollection<TDiscriminator, IPod<TDiscriminator,object, JsonPodCollection<TDiscriminator>>>// IEnumerable<(TDiscriminator, JsonValue)> 
	where TDiscriminator : notnull
{
	public JsonPodCollection(){}
	// public JsonPodCollection(JsonPod<TDiscriminator, object, TDiscriminator>[] pods){}
	internal Dictionary<TDiscriminator, JsonValue> InternalDictionary { get; } = new();
	[JsonIgnore]
	public int Count => InternalDictionary.Count;
	
	
	public void Add<TPayload>(TDiscriminator discriminator, TPayload payload) //where TPayload : notnull 
		=> Add(new JsonPod<TDiscriminator, TPayload>(discriminator, payload));
	void IPodCollection<TDiscriminator, IPod<TDiscriminator, object, JsonPodCollection<TDiscriminator>>>.Add<TPayload>(IPod<TDiscriminator, object, JsonPodCollection<TDiscriminator>> pod)
	{
		if (pod is JsonPod<TDiscriminator, TPayload> jsonPod)
			Add(jsonPod);
		else
			throw new ArgumentException($"'{GetType().GetSignature()}' only allow '{typeof(JsonPod<TDiscriminator, TPayload>).GetSignature()}' items");
	}
	public void Add<TPayload>(JsonPod<TDiscriminator, TPayload> pod)
		=> InternalDictionary.Add(pod.Discriminator, JsonValue.Create(pod) ?? throw new ArgumentException($"The pod of type '{pod.GetType().GetSignature()}' cannot be serialized"));
	
	
	public void Replace<TPayload>(TDiscriminator discriminator, TPayload payload) //where TPayload : notnull 
		=> Replace<TPayload>(new(discriminator, payload));
	public void Replace<TPayload>(JsonPod<TDiscriminator, TPayload> pod) //where TPayload : notnull
	{
		if (!InternalDictionary.ContainsKey(pod.Discriminator)) throw new KeyNotFoundException($"No header with discriminator '{pod.Discriminator}' was found to replace");
		InternalDictionary[pod.Discriminator] = JsonValue.Create(pod) ?? throw new ArgumentException($"The pod of type '{pod.GetType().GetSignature()}' cannot be serialized");
	}
	
	
	public bool Has(TDiscriminator discriminator) => InternalDictionary.ContainsKey(discriminator);
	
	
	IPod<TDiscriminator, object, JsonPodCollection<TDiscriminator>>? IPodCollection<TDiscriminator, IPod<TDiscriminator,object, JsonPodCollection<TDiscriminator>>>.GetPod<TPayload>(TDiscriminator discriminator)
		=> GetPod<TPayload>(discriminator)?.CastWithPayload<object>();
	public JsonPod<TDiscriminator, TPayload>? GetPod<TPayload>(TDiscriminator discriminator)
		//where TPayload : notnull
		=> InternalDictionary[discriminator].Deserialize<JsonPod<TDiscriminator, TPayload>>();
	bool IPodCollection<TDiscriminator, IPod<TDiscriminator, object, JsonPodCollection<TDiscriminator>>>.TryGetPod<TPayload>(
		TDiscriminator discriminator, [NotNullWhen(true)] out IPod<TDiscriminator, object, JsonPodCollection<TDiscriminator>>? pod)
	{
		TryGetPod<TPayload>(discriminator, out var res);
		pod = res?.CastWithPayload<object>();
		return res is not null;
	}
	public bool TryGetPod<TPayload>(TDiscriminator discriminator, out JsonPod<TDiscriminator, TPayload>? pod)
	{
		var res = GetPod<TPayload>(discriminator);
		pod = res;
		return res is not null;
	}
	
	
	public TPayload? GetPayload<TPayload>(TDiscriminator discriminator)
	{
		var res = InternalDictionary[discriminator].Deserialize<JsonPod<TDiscriminator, TPayload>>();
		return res is null ? default : res.Payload;
	}
	// bool IPodCollection<TDiscriminator, IPod<TDiscriminator, object, JsonPodCollection<TDiscriminator>>>.TryGetPayload<TPayload>(TDiscriminator discriminator, [NotNullWhen(true)] out TPayload? payload)
	// 	where TPayload : default
	// {
	// 	TryGetPayload<TPayload>(discriminator, out var res);
	// 	payload = res;
	// 	return res is not null;
	// }
	public bool TryGetPayload<TPayload>(TDiscriminator discriminator, [NotNullWhen(true)] out TPayload? payload)
	{
		var res = InternalDictionary[discriminator].Deserialize<TPayload>();
		payload = res;
		return res is not null;
	}
	IPod<TDiscriminator, object, JsonPodCollection<TDiscriminator>> IPodCollection<TDiscriminator, IPod<TDiscriminator, object, JsonPodCollection<TDiscriminator>>>.this[TDiscriminator discriminator] =>
		GetPod<object>(discriminator) ?? throw new ArgumentException($"Discriminator '{discriminator}' not found");

	public IEnumerator<IPod<TDiscriminator,object, JsonPodCollection<TDiscriminator>>> GetEnumerator() 
		=> InternalDictionary.Select(_ => _.Value.Deserialize<IPod<TDiscriminator,object,JsonPodCollection<TDiscriminator>>>()!).GetEnumerator();
}