using System.Text;
using System.Text.Json.Nodes;

namespace Fuxion.Json;

public static class JsonPodExtensions
{
	public static IPodBuilder<JsonPod<TDiscriminator, T>> ToJson<TDiscriminator, T>(this IPodBuilder<ICrossPod<TDiscriminator, T, T>> me)
		where TDiscriminator : notnull
		where T : notnull =>
		new PodBuilder<TDiscriminator, T, JsonValue, JsonPod<TDiscriminator, T>>(new (me.Pod.Discriminator, me.Pod.Inside()));
	
	public static IPodBuilder<ICrossPod<TDiscriminator, byte[], byte[]>> ToUtf8<TDiscriminator, T>(this IPodBuilder<JsonPod<TDiscriminator, T>> me)
		where TDiscriminator : notnull 
		where T : notnull =>
		new PodBuilder<TDiscriminator, byte[], byte[], ICrossPod<TDiscriminator, byte[], byte[]>>(new BypassPod<TDiscriminator, byte[], byte[]>(me.Pod.Discriminator, Encoding.UTF8.GetBytes(me.Pod.ToJson())));

	public static JsonPod<TDiscriminator, object> FromJsonPod<TDiscriminator>(this string me) where TDiscriminator : notnull =>
		me.FromJson<JsonPod<TDiscriminator, object>>()
		?? throw new FormatException($"The string couldn't be deserialized as '{typeof(JsonPod<TDiscriminator, object>).GetSignature()}'");
	
	/// <summary>
	/// Returns a pod builder from a JsonPod
	/// </summary>
	public static IPodBuilder<JsonPod<TDiscriminator, TPayload>> BuildPod<TDiscriminator, TPayload>(this JsonPod<TDiscriminator, TPayload> me)
		where TDiscriminator : notnull
		where TPayload : notnull =>
		new PodBuilder<TDiscriminator, TPayload, JsonValue, JsonPod<TDiscriminator, TPayload>>(me);
	
	public static IPodBuilder<JsonPod<TDiscriminator, object>> FromJson<TDiscriminator>(this IPodBuilder<ICrossPod<TDiscriminator, string, string>> me) 
		where TDiscriminator : notnull
	{
		var pod = me.Pod.Outside().FromJsonPod<TDiscriminator>();
		return new PodBuilder<TDiscriminator, object, JsonValue, JsonPod<TDiscriminator, object>>(pod);
	}

	public static IPodBuilder<ICrossPod<TDiscriminator, byte[], byte[]>> FromBytes<TDiscriminator, T>(this IPodBuilder<JsonPod<TDiscriminator, T>> me) where TDiscriminator : notnull =>
		new PodBuilder<TDiscriminator, byte[], byte[], ICrossPod<TDiscriminator, byte[], byte[]>>(new BypassPod<TDiscriminator, byte[], byte[]>(me.Pod.Discriminator,
			me.Pod.As<byte[]>() ?? throw new ArgumentException($"'{me.GetType().GetSignature()}' payload couldn't be deserialized as '{typeof(byte[]).GetSignature()}'")));
	
}