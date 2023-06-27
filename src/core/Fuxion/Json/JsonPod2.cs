using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using JsonPodConverter = Fuxion.Json.JsonPod2Converter<Fuxion.Json.JsonPod2<string,string>,string,string>;

namespace Fuxion.Json;

[JsonConverter(typeof(JsonPodNode2ConverterFactory))]
public class JsonNodePod2<TDiscriminator>(TDiscriminator discriminator, object value) : IPod2<TDiscriminator, JsonNode>, IPod2<TDiscriminator, string>
	where TDiscriminator : notnull
{
	// ATTENTION: This constructor cannot be removed, it is needed for deserialization
	protected JsonNodePod2() : this(default!, default!) { }
	// ATTENTION: The init setter cannot be removed, it is needed for deserialization
	public TDiscriminator Discriminator { get; init; } = discriminator;
	// ATTENTION: The init setter cannot be removed, it is needed for deserialization
	public JsonNode Payload { get; init; } = CreateValue(value);
	string IPod2<TDiscriminator, string>.Payload => this;
	static JsonNode CreateValue(object payload)
	{
		// TODO ver si podemos mejorar este tratamiento de nullable
		if (payload is null) return null!;
		JsonSerializerOptions options = new();
		options.Converters.Add(new JsonPodNode2ConverterFactory());
		var node = JsonSerializer.SerializeToNode(payload, options)
		 	?? throw new SerializationException($"Serialization returns null");
		return node;
	}
	public override string ToString() => this.SerializeToJson();
	public static implicit operator string(JsonNodePod2<TDiscriminator> pod) => pod.SerializeToJson();
	
	public T? As<T>()
	{
		JsonSerializerOptions options = new();
		options.Converters.Add(new JsonPodNode2ConverterFactory());
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
[JsonConverter(typeof(JsonPod2ConverterFactory))]
public class JsonPod2<TDiscriminator, TPayload> : ICollectionPod2<TDiscriminator, TPayload>//Pod2<TDiscriminator, TPayload, JsonPodCollection2<TDiscriminator>>
	where TDiscriminator : notnull
{
	[JsonConstructor]
	protected JsonPod2()
	{
		Discriminator = default!;
		_payloadValue = default!;
	}
	internal JsonPod2(TDiscriminator discriminator, JsonValue payloadValue) : this()
	{
		Discriminator = discriminator;
		PayloadValue = payloadValue;
	}
	public JsonPod2(TDiscriminator discriminator, TPayload payload) : this()
	{
		Discriminator = discriminator;
		Payload = payload;
		PayloadValue = CreateValue(payload);
	}
	public JsonPod2(IPod2<TDiscriminator, TPayload> pod) : this()
	{
		Discriminator = pod.Discriminator;
		Payload = pod.Payload;
		if (pod.Payload is not null) PayloadValue = CreateValue(pod.Payload);
		if (pod is not ICollectionPod2<TDiscriminator, TPayload> col) return;
		foreach (var item in col)
			Add(new JsonPod2<TDiscriminator, object>(item));
	}
	TPayload? _payload;
	JsonValue? _payloadValue;
	
	#region Collection
	internal Dictionary<TDiscriminator, JsonValue> InternalDictionary { get; } = new();
	[JsonIgnore]
	public int Count => InternalDictionary.Count;
	public bool Has(TDiscriminator discriminator) => InternalDictionary.ContainsKey(discriminator);
	public IPod2<TDiscriminator, object> this[TDiscriminator discriminator] =>
		//new JsonPod2<TDiscriminator, object>(discriminator,InternalDictionary[discriminator])
		InternalDictionary[discriminator].ToString().DeserializeFromJson<JsonPod2<TDiscriminator, object>>() 
		?? throw new ArgumentException($"'{nameof(JsonValue)}' for discriminator '{discriminator}' couldn't be deserialized as '{typeof(JsonPod2<TDiscriminator, object>).GetSignature()}'");
	public TPod ItemAs<TPod>(TDiscriminator discriminator)=>
		InternalDictionary[discriminator].ToString().DeserializeFromJson<TPod>() 
		?? throw new ArgumentException($"'{nameof(JsonValue)}' for discriminator '{discriminator}' couldn't be deserialized as '{typeof(JsonPod2<TDiscriminator, object>).GetSignature()}'");
	public void Add(IPod2<TDiscriminator, object> pod)
	{
		if (pod is JsonPod2<TDiscriminator, object> oo)
		{
			InternalDictionary.Add(pod.Discriminator, JsonValue.Create(oo)!);
		
		// if (pod.GetType()
		// 	.IsSubclassOfRawGeneric(typeof(JsonPod2<,>)))
		// {
		// 	var obj = pod.GetType()
		// 		.GetProperty(nameof(PayloadValue), BindingFlags.Public | BindingFlags.NonPublic| BindingFlags.Instance)
		// 		?.GetValue(pod);
		// 	if (obj is null) throw new JsonException("Property PayloadValue cannot be obtained");
		// 		InternalDictionary.Add(pod.Discriminator, JsonValue.Create(pod)!);
		} else
		{
			var jsonPod = new JsonPod2<TDiscriminator, object>(pod.Discriminator, pod);
			if (jsonPod.PayloadValue is null) throw new InvalidProgramException($"PayloadValue cannot be null here");
			InternalDictionary.Add(jsonPod.Discriminator, jsonPod.PayloadValue);
			// if(pod.GetType().IsSubclassOfRawGeneric(typeof(JsonPod2<,>)))
			// 	InternalDictionary.Add(pod.Discriminator, new JsonPod2<TDiscriminator, object>(pod.Discriminator, pod).PayloadValue);
			// else throw new ArgumentException($"In '{this.GetType().GetSignature()}' only '{typeof(JsonPod2<TDiscriminator, object>).GetSignature()}' can be added");
		}
	}
	public bool Remove(TDiscriminator discriminator) => InternalDictionary.Remove(discriminator);
	public IEnumerator<IPod2<TDiscriminator, object>> GetEnumerator() =>
		InternalDictionary.Select(_ => new JsonPod2<TDiscriminator, object>(_.Key, _.Value)).GetEnumerator();
	#endregion
	
	[JsonPropertyName(JsonPodConverter.DISCRIMINATOR_LABEL)]
	public TDiscriminator Discriminator { get; internal set; }
	[JsonPropertyName(JsonPodConverter.PAYLOAD_LABEL)]
	protected internal JsonValue? PayloadValue
	{
		get => _payloadValue;
		set
		{
			_payloadValue = value;
			if (Payload == null && PayloadValue != null)
				try
				{
					Payload = PayloadValue.Deserialize<TPayload>()!;
				} catch { }
		}
	}
	[JsonIgnore]
	public bool PayloadHasValue { get; private set; }
	[JsonIgnore]
	public TPayload Payload
	{
		get => _payload!;
		protected set
		{
			_payload = value;
			PayloadHasValue = true;
		}
	}
	// public JsonPodCollection2<TDiscriminator> Headers { get; private set; } = new();
	static JsonValue CreateValue(TPayload payload)
	{
		if (payload is null) throw new ArgumentNullException(nameof(payload), $@"'{nameof(payload)}' could not be null");
		var met = (typeof(JsonValue)
				.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(m => m.Name == nameof(JsonValue.Create) && m.GetGenericArguments().Any() && m.GetParameters().Length == 2)
			?? throw new InvalidProgramException($"The '{nameof(JsonValue)}.{nameof(JsonValue.Create)}<T>()' method could not be determined"))
			.MakeGenericMethod(payload.GetType());
		return (JsonValue)(met.Invoke(null, new object?[] {
				payload, null
			})
			?? throw new InvalidProgramException($"The '{nameof(JsonValue)}' could not be created with '{met.GetSignature()}' method."));
	}
	public static implicit operator TPayload?(JsonPod2<TDiscriminator, TPayload> pod) => pod.Payload;
	public JsonPod2<TDiscriminator, T>? CastWithPayload<T>()
		where T : notnull
	{
		if (PayloadValue == null) return null;
		var res = PayloadValue.Deserialize<T>();
		if (res == null) return null;
		// TODO Falta agregar los items
		return new(Discriminator, res);
	}
	public T? As<T>()
	{
		if (PayloadValue == null) return default;
		var res = PayloadValue.Deserialize<T>();
		return res ?? default;
	}
	public bool TryAs<T>([NotNullWhen(true)] out T? payload)
	{
		var res = As<T>();
		payload = res;
		return res is not null;
	}
	public object? As(Type type, JsonSerializerOptions? options = null) => PayloadValue.Deserialize(type, options);
	public bool Is<T>() => Is(typeof(T));
	public bool Is(Type type)
	{
		try
		{
			As(type);
			return true;
		} catch (Exception ex)
		{
			return false;
		}
	}
}