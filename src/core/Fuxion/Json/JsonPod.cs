using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Fuxion.Json;

[JsonConverter(typeof(JsonPodConverterFactory))]
public class JsonPod<TPayload, TKey>
{
	[JsonConstructor]
	protected JsonPod()
	{
		PayloadKey = default!;
		_Payload = default!;
		_PayloadValue = null!;
	}
	public JsonPod(TPayload payload, TKey key) : this()
	{
		PayloadKey = key;
		Payload = payload;
		PayloadValue = CreateValue(payload);
	}
	TPayload? _Payload;
	JsonValue _PayloadValue;
	public TKey PayloadKey { get; internal set; }
	[JsonPropertyName(nameof(Payload))]
	protected internal JsonValue PayloadValue
	{
		get => _PayloadValue;
		set
		{
			_PayloadValue = value;
			if (Payload == null && PayloadValue != null)
				try
				{
					Payload = PayloadValue.Deserialize<TPayload>();
				} catch { }
		}
	}
	[JsonIgnore]
	public bool PayloadHasValue { get; private set; }
	[JsonIgnore]
	public TPayload? Payload
	{
		get => _Payload;
		private set
		{
			_Payload = value;
			PayloadHasValue = true;
		}
	}
	JsonValue CreateValue(TPayload payload)
	{
		var met = typeof(JsonValue).GetMethods(BindingFlags.Static | BindingFlags.Public)
			.Where(m => m.Name == nameof(JsonValue.Create) && m.GetGenericArguments().Any() && m.GetParameters().Count() == 2).FirstOrDefault();
		if (met is null) throw new InvalidProgramException("The JsonValue.Create<T>() method could not be determined");
		if (payload is null) throw new ArgumentNullException("payload could not be null");
		var met2 = met.MakeGenericMethod(payload.GetType());
		var res = met2.Invoke(null, new object?[] {
			payload, null
		});
		if (res is null) throw new InvalidProgramException("The JsonValue could not be created");
		return (JsonValue)res;
	}
	public static implicit operator TPayload?(JsonPod<TPayload, TKey> pod) => pod.Payload;
	public JsonPod<T, TKey>? CastWithPayload<T>()
	{
		if (PayloadValue == null) return null;
		var res = PayloadValue.Deserialize<T>();
		if (res == null) return null;
		return new(res, PayloadKey);
	}
	public T? As<T>()
	{
		if (PayloadValue == null) return default;
		var res = PayloadValue.Deserialize<T>();
		if (res == null) return default;
		return res;
	}
	public object? As(Type type) => PayloadValue.Deserialize(type);
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