using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Fuxion.Json;

[JsonConverter(typeof(JsonPod2ConverterFactory))]
public class JsonPod2<TDiscriminator, TPayload> : IPod2<TDiscriminator, TPayload, JsonPodCollection2<TDiscriminator>>
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
	TPayload? _payload;
	JsonValue _payloadValue;
	public TDiscriminator Discriminator { get; internal set; }
	[JsonPropertyName(nameof(Payload))]
	protected internal JsonValue PayloadValue
	{
		get => _payloadValue;
		set
		{
			_payloadValue = value;
			if (Payload == null && PayloadValue != null)
				try
				{
					Payload = PayloadValue.Deserialize<TPayload>();
				} catch { }
		}
	}
	// [JsonIgnore]
	// public JsonValue? Raw => PayloadValue;
	// JsonValue ICrossPod<TDiscriminator, TPayload, JsonValue>.Inside() => PayloadValue;
	// TPayload ICrossPod<TDiscriminator, TPayload, JsonValue>.Outside() => Payload ?? throw new ArgumentException($"Payload is null");
	[JsonIgnore]
	public bool PayloadHasValue { get; private set; }
	[JsonIgnore]
	public TPayload? Payload
	{
		get => _payload;
		protected set
		{
			_payload = value;
			PayloadHasValue = true;
		}
	}
	public JsonPodCollection2<TDiscriminator> Headers { get; private set; } = new();
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
	public JsonPod<TDiscriminator, T>? CastWithPayload<T>()
		where T : notnull
	{
		if (PayloadValue == null) return null;
		var res = PayloadValue.Deserialize<T>();
		if (res == null) return null;
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
			Debug.WriteLine("" + ex.Message);
			return false;
		}
	}
}