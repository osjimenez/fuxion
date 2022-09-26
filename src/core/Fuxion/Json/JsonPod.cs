﻿namespace Fuxion.Json;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

public class JsonPod<TPayload, TKey>
{
	[JsonConstructor]
	protected JsonPod()
	{
		PayloadKey = default!;
		_Payload = default!;
		_PayloadJRaw = null!;
	}
	public JsonPod(TPayload payload, TKey key) : this()
	{
		PayloadKey = key;
		Payload = payload;
		PayloadJRaw = payload.ToJson();
	}
	public TKey PayloadKey { get; private set; }
	
	private string _PayloadJRaw;
	[JsonPropertyName(nameof(Payload))]
	internal string PayloadJRaw
	{
		get => _PayloadJRaw;
		set
		{
			_PayloadJRaw = value;
			if (Payload == null && PayloadJRaw != null)
			{
				Payload = PayloadJRaw.FromJson<TPayload>() ?? throw new InvalidDataException("The PayloadJRaw value hasn't a representative string"); ;


				//var wasFailed = false;
				//var rr = new object();
				//var str = PayloadJRaw.Value.ToString();
				//if (str == null || str.IsNullOrEmpty()) throw new InvalidDataException("The PayloadJRaw value hasn't a representative string");
				//var res = str.FromJson<TPayload>(settings: new JsonSerializerOptions
				//{
					
				//	Error = delegate (object? sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
				//	{
				//		wasFailed = true;
				//		args.ErrorContext.Handled = true;
				//	}
				//});
				//if (res != null && !wasFailed)
				//	Payload = res;
			}
		}
	}
	[JsonIgnore]
	public bool PayloadHasValue { get; private set; }

	private TPayload _Payload;
	[JsonIgnore]
	public TPayload Payload
	{
		get => _Payload;
		private set
		{
			_Payload = value;
			PayloadHasValue = true;
		}
	}

	public static implicit operator TPayload(JsonPod<TPayload, TKey> payload) => payload.Payload;
	public JsonPod<T, TKey>? CastWithPayload<T>()
	{
		if (PayloadJRaw == null) return null;
		var res = PayloadJRaw.FromJson<T>();
		if (res == null) return null;
		return new JsonPod<T, TKey>(res, PayloadKey);
	}
	public T? As<T>()
	{
		if (PayloadJRaw == null) return default;
		var res = PayloadJRaw.FromJson<T>();
		if (res == null) return default;
		return res;
	}
	public object? As(Type type) => PayloadJRaw.FromJson(type);

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

//public class JsonPod<TPayload, TKey>
//{
//	[JsonConstructor]
//	protected JsonPod()
//	{
//		PayloadKey = default!;
//		_Payload = default!;
//		_PayloadJRaw = null!;
//	}
//	public JsonPod(TPayload payload, TKey key) : this()
//	{
//		PayloadKey = key;
//		Payload = payload;
//		PayloadJRaw = new JRaw(payload?.ToJson());
//	}
//	[JsonProperty]
//	public TKey PayloadKey { get; private set; }

//	private JRaw _PayloadJRaw;
//	[JsonProperty(PropertyName = nameof(Payload))]
//	internal JRaw PayloadJRaw
//	{
//		get => _PayloadJRaw;
//		set
//		{
//			_PayloadJRaw = value;
//			if (Payload == null && PayloadJRaw.Value != null)
//			{
//				var wasFailed = false;
//				var rr = new object();
//				var str = PayloadJRaw.Value.ToString();
//				if (str == null || str.IsNullOrEmpty()) throw new InvalidDataException("The PayloadJRaw value hasn't a representative string");
//				var res = str.FromJson<TPayload>(settings: new JsonSerializerSettings
//				{
//					Error = delegate (object? sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
//					{
//						wasFailed = true;
//						args.ErrorContext.Handled = true;
//					}
//				});
//				if (res != null && !wasFailed)
//					Payload = res;
//			}
//		}
//	}
//	[JsonIgnore]
//	public bool PayloadHasValue { get; private set; }

//	private TPayload _Payload;
//	[JsonIgnore]
//	public TPayload Payload
//	{
//		get => _Payload;
//		private set
//		{
//			_Payload = value;
//			PayloadHasValue = true;
//		}
//	}

//	public static implicit operator TPayload(JsonPod<TPayload, TKey> payload) => payload.Payload;
//	public JsonPod<T, TKey>? CastWithPayload<T>()
//	{
//		var payload = PayloadJRaw.Value?.ToString();
//		if (payload == null) return null;
//		var res = payload.FromJson<T>();
//		if (res == null) return null;
//		return new JsonPod<T, TKey>(res, PayloadKey);
//	}
//	public T? As<T>()
//	{
//		var payload = PayloadJRaw.Value?.ToString();
//		if (payload == null) return default;
//		var res = payload.FromJson<T>();
//		if (res == null) return default;
//		return res;
//	}
//	public object? As(Type type) => PayloadJRaw.Value?.ToString()?.FromJson(type);

//	public bool Is<T>() => Is(typeof(T));
//	public bool Is(Type type)
//	{
//		try
//		{
//			As(type);
//			return true;
//		}
//		catch (Exception ex)
//		{
//			Debug.WriteLine("" + ex.Message);
//			return false;
//		}
//	}
//}