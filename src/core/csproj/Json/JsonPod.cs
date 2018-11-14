using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Fuxion.Json
{
	public static class JsonPodExtensions
	{
		public static JsonPod<TPayload, TKey> ToJsonPod<TPayload, TKey>(this TPayload me, TKey key, string name = null) where TPayload : class => JsonPod<TPayload, TKey>.Create(me, key, name);
		public static JsonPod<TPayload, TKey> FromJsonPod<TPayload, TKey>(this string me) where TPayload : class => me.FromJson<JsonPod<TPayload, TKey>>();
	}
	public class JsonPod<TPayload, TKey> //where TPayload : class
	{
		protected JsonPod() { }
		internal static JsonPod<TPayload, TKey> Create(TPayload payload, TKey key, string name = null)
			=> new JsonPod<TPayload, TKey>
			{
				Key = key,
				Name = name ?? key.ToString(),
				Payload = payload, // ?? throw new ArgumentException("Payload cannot be null", nameof(payload)),
				PayloadJRaw = new JRaw(payload.ToJson())
			};

		public string Name { get; private set; }
		public TKey Key { get; private set; }

		JRaw _PayloadJRaw;
		[JsonProperty(PropertyName = nameof(Payload))]
		protected JRaw PayloadJRaw
		{
			get => _PayloadJRaw;
			set
			{
				_PayloadJRaw = value;
				if (Payload == null) Payload = PayloadJRaw.Value.ToString().FromJson<TPayload>();
			}
		}
		[JsonIgnore]
		protected TPayload Payload { get; set; }

		public static implicit operator TPayload(JsonPod<TPayload, TKey> payload)
		{
			return payload.Payload;
		}
		public JsonPod<T, TKey> CastToPayload<T>() => JsonPod<T, TKey>.Create(PayloadJRaw.Value.ToString().FromJson<T>(), Key, Name);
		public T As<T>() => PayloadJRaw.Value.ToString().FromJson<T>();
		public object As(Type type) => PayloadJRaw.Value.ToString().FromJson(type);
	}
}