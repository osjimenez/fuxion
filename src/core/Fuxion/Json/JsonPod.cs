namespace Fuxion.Json;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonPodConverterFactory))]
public class JsonPod<TPayload, TKey>
{
	[JsonConstructor]
	protected JsonPod()
	{
		PayloadKey = default!;
		_Payload = default!;
		_PayloadRaw = null!;
	}
	public JsonPod(TPayload payload, TKey key) : this()
	{
		PayloadKey = key;
		Payload = payload;
		PayloadRaw = payload.ToJson();
	}
	public TKey PayloadKey { get; private set; }
	
	private string _PayloadRaw;
	[JsonPropertyName(nameof(Payload))]
	protected internal string PayloadRaw
	{
		get => _PayloadRaw;
		set
		{
			_PayloadRaw = value;
			if (Payload == null && PayloadRaw != null)
			{
				try
				{
					Payload = PayloadRaw.FromJson<TPayload>();
				}
				catch { }
			}
		}
	}
	[JsonIgnore]
	public bool PayloadHasValue { get; private set; }

	private TPayload? _Payload;
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

	public static implicit operator TPayload?(JsonPod<TPayload, TKey> pod) => pod.Payload;
	public JsonPod<T, TKey>? CastWithPayload<T>()
	{
		if (PayloadRaw == null) return null;
		var res = PayloadRaw.FromJson<T>();
		if (res == null) return null;
		return new JsonPod<T, TKey>(res, PayloadKey);
	}
	public T? As<T>()
	{
		if (PayloadRaw == null) return default;
		var res = PayloadRaw.FromJson<T>();
		if (res == null) return default;
		return res;
	}
	public object? As(Type type) => PayloadRaw.FromJson(type);

	public bool Is<T>() => Is(typeof(T));
	public bool Is(Type type)
	{
		try
		{
			As(type);
			return true;
		}
		catch (Exception ex)
		{
			Debug.WriteLine("" + ex.Message);
			return false;
		}
	}
}