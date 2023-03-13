using System.Text;
using System.Text.Json.Nodes;

namespace Fuxion.Json;

public static class JsonPodExtensions
{
	public static IPodBuilder<JsonPod<TDiscriminator, T>> Json<TDiscriminator, T>(this IPodBuilder<IPod<TDiscriminator, T, T>> me)
		where TDiscriminator : notnull
		where T : notnull =>
		new PodBuilder<TDiscriminator, T, JsonValue, JsonPod<TDiscriminator, T>>(new (me.Pod.Discriminator, me.Pod.Inside()));
	
	public static IPodBuilder<IPod<TDiscriminator, T, byte[]>> Utf8<TDiscriminator, T>(this IPodBuilder<IPod<TDiscriminator, T, JsonValue>> me)
		where TDiscriminator : notnull 
		where T : notnull =>
		new PodBuilder<TDiscriminator, T, byte[], IPod<TDiscriminator, T, byte[]>>(new Pod<TDiscriminator, T, byte[]>(me.Pod.Discriminator, Encoding.UTF8.GetBytes(me.Pod.ToJson())));
	
	public static IPodBuilder<IPod<TDiscriminator, T, byte[]>> Base64<TDiscriminator, T>(this IPodBuilder<IPod<TDiscriminator, string, JsonValue>> me)
		where TDiscriminator : notnull 
		where T : notnull =>
		new PodBuilder<TDiscriminator, T, byte[], IPod<TDiscriminator, T, byte[]>>(new Pod<TDiscriminator, T, byte[]>(me.Pod.Discriminator, Encoding.UTF8.GetBytes(me.Pod.ToJson())));

	public static JsonPod<TDiscriminator, object> FromJsonPod<TDiscriminator>(this string me) where TDiscriminator : notnull =>
		me.FromJson<JsonPod<TDiscriminator, object>>()
		?? throw new FormatException($"The string couldn't be deserialized as '{typeof(JsonPod<TDiscriminator, object>).GetSignature()}'");
}