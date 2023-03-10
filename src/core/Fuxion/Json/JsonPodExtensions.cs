namespace Fuxion.Json;

public static class JsonPodExtensions
{
	public static JsonPod<TDiscriminator, TPayload> ToJsonPod<TDiscriminator, TPayload>(this TPayload me, TDiscriminator key) 
		// where TPayload : notnull
		where TDiscriminator : notnull
		=> new(key, me);
	public static JsonPod<TDiscriminator, TPayload>? FromJsonPod<TDiscriminator, TPayload>(this string me) 
		// where TPayload : notnull
		where TDiscriminator : notnull
		=> me.FromJson<JsonPod<TDiscriminator, TPayload>>();
}