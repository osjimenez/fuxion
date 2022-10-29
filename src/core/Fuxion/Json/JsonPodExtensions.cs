namespace Fuxion.Json;

public static class JsonPodExtensions
{
	public static JsonPod<TPayload, TKey>  ToJsonPod<TPayload, TKey>(this   TPayload me, TKey key) where TPayload : class => new(me, key);
	public static JsonPod<TPayload, TKey>? FromJsonPod<TPayload, TKey>(this string   me) where TPayload : class => me.FromJson<JsonPod<TPayload, TKey>>();
}