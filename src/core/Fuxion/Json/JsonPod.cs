using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Fuxion.Json
{
	public static class JsonPodExtensions
	{
		public static JsonPod<TPayload, TKey> ToJsonPod<TPayload, TKey>(this TPayload me, TKey key) where TPayload : class => new JsonPod<TPayload, TKey>(me, key);
		public static JsonPod<TPayload, TKey>? FromJsonPod<TPayload, TKey>(this string me) where TPayload : class => me.FromJson<JsonPod<TPayload, TKey>>();
	}
	public class JsonPod<TPayload, TKey>
	{
		[JsonConstructor]
		protected JsonPod() {
			PayloadKey = default!;
			_Payload = default!;
			_PayloadJRaw = null!;
		}
		public JsonPod(TPayload payload, TKey key) : this()
		{
			PayloadKey = key;
			Payload = payload;
			PayloadJRaw = new JRaw(payload?.ToJson());
		}
		[JsonProperty]
		public TKey PayloadKey { get; private set; }
		JRaw _PayloadJRaw;
		[JsonProperty(PropertyName = nameof(Payload))]
		internal JRaw PayloadJRaw
		{
			get => _PayloadJRaw;
			set
			{
				_PayloadJRaw = value;
				if (Payload == null && PayloadJRaw.Value != null)
				{
					bool wasFailed = false;
					object rr = new object();
					var str = PayloadJRaw.Value.ToString();
					if (str== null || str.IsNullOrEmpty()) throw new System.IO.InvalidDataException("The PayloadJRaw value hasn't a representative string");
					var res  = str.FromJson<TPayload>(new JsonSerializerSettings
					{
						Error = delegate (object? sender, ErrorEventArgs args)
						{
							wasFailed = true;
							args.ErrorContext.Handled = true;
						}
					});
					if (res != null && !wasFailed)
						Payload = res;
				}
			}
		}
		[JsonIgnore]
		public bool PayloadHasValue { get; private set; }
		TPayload _Payload;
		[JsonIgnore]
		public TPayload Payload {
			get => _Payload;
			private set
			{
				_Payload = value;
				PayloadHasValue = true;
			}
		}

		public static implicit operator TPayload(JsonPod<TPayload, TKey> payload)=> payload.Payload;
		public JsonPod<T, TKey>? CastWithPayload<T>()
		{
			var payload = PayloadJRaw.Value?.ToString();
			if (payload == null) return null;
			var res = payload.FromJson<T>();
			if (res == null) return null;
			return new JsonPod<T, TKey>(res, PayloadKey);
		}
		public T? As<T>()
		{
			var payload = PayloadJRaw.Value?.ToString();
			if (payload == null) return default;
			var res = payload.FromJson<T>();
			if (res == null) return default;
			return res;
		}
		public object? As(Type type) => PayloadJRaw.Value?.ToString()?.FromJson(type);

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
}