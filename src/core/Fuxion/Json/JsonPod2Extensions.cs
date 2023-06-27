using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Nodes;

namespace Fuxion.Json;

public static class JsonPod2Extensions
{
	// public static IPodBuilder2<JsonNodePod2<TDiscriminator>> BuildJsonNodePod2<TDiscriminator, T>(this T me, TDiscriminator discriminator) 
	// 	where TDiscriminator : notnull 
	// 	where T : notnull =>
	// 	new PodBuilder2<TDiscriminator, JsonNode, JsonNodePod2<TDiscriminator>>(new JsonNodePod2<TDiscriminator>(discriminator, me));
	public static IPodBuilder2<JsonNodePod2<TDiscriminator>> ToJsonNode<TDiscriminator, TPayload>(this IPodBuilder2<IPod2<TDiscriminator, TPayload>> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		where TPayload : notnull =>
		new PodBuilder2<TDiscriminator, JsonNode, JsonNodePod2<TDiscriminator>>(new (discriminator, me.Pod));
	public static IPodBuilder2<JsonNodePod2<TDiscriminator>> ToJsonNode<TDiscriminator, TPayload>(this IPodPreBuilder2<TPayload> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		where TPayload : notnull =>
		new PodBuilder2<TDiscriminator, JsonNode, JsonNodePod2<TDiscriminator>>(new (discriminator, me.Payload));
	public static IPodBuilder2<IPod2<TDiscriminator, byte[]>> ToUtf8Bytes<TDiscriminator>(this IPodBuilder2<IPod2<TDiscriminator, string>> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		=> new PodBuilder2<TDiscriminator, byte[], IPod2<TDiscriminator, byte[]>>(new Pod2<TDiscriminator, byte[]>(discriminator, Encoding.UTF8.GetBytes(me.Pod.Payload)));
	
	public static IPodBuilder2<IPod2<TDiscriminator, string>> FromUtf8Bytes<TDiscriminator>(this IPodBuilder2<IPod2<TDiscriminator, byte[]>> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		=> new PodBuilder2<TDiscriminator, string, IPod2<TDiscriminator, string>>(new Pod2<TDiscriminator, string>(discriminator, Encoding.UTF8.GetString(me.Pod.Payload)));
	public static IPodBuilder2<IPod2<TDiscriminator, string>> FromUtf8Bytes<TDiscriminator>(this IPodPreBuilder2<byte[]> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		=> new PodBuilder2<TDiscriminator, string, IPod2<TDiscriminator, string>>(new Pod2<TDiscriminator, string>(discriminator, Encoding.UTF8.GetString(me.Payload)));
	
	public static IPodBuilder2<JsonNodePod2<TDiscriminator>> FromJsonNode<TDiscriminator>(this IPodBuilder2<IPod2<TDiscriminator, string>> me)
		where TDiscriminator : notnull
		=> new PodBuilder2<TDiscriminator, JsonNode, JsonNodePod2<TDiscriminator>>(me.Pod.Payload.DeserializeFromJson<JsonNodePod2<TDiscriminator>>()
			?? throw new SerializationException($"string couldn't be deserialized"));









	public static IPodBuilder2<JsonPod2<TDiscriminator, T>> ToJson<TDiscriminator, T>(this IPodBuilder2<IPod2<TDiscriminator, T>> me)
		where TDiscriminator : notnull
		where T : notnull =>
		new PodBuilder2<TDiscriminator, T, JsonPod2<TDiscriminator, T>>(new (me.Pod));
	

	
	public static IPodBuilder2<IPod2<TDiscriminator, byte[]>> ToUtf8<TDiscriminator, T>(this IPodBuilder2<JsonPod2<TDiscriminator, T>> me)
		where TDiscriminator : notnull 
		where T : notnull =>
		new PodBuilder2<TDiscriminator, byte[], IPod2<TDiscriminator, byte[]>>(new Pod2<TDiscriminator, byte[]>(me.Pod.Discriminator, Encoding.UTF8.GetBytes(me.Pod.SerializeToJson())));

	public static JsonPod2<TDiscriminator, object> FromJsonPod2<TDiscriminator>(this string me) where TDiscriminator : notnull =>
		me.DeserializeFromJson<JsonPod2<TDiscriminator, object>>()
		?? throw new FormatException($"The string couldn't be deserialized as '{typeof(JsonPod2<TDiscriminator, object>).GetSignature()}'");
	
	/// <summary>
	/// Returns a pod builder from a JsonPod
	/// </summary>
	public static IPodBuilder2<JsonPod2<TDiscriminator, TPayload>> BuildPod<TDiscriminator, TPayload>(this JsonPod2<TDiscriminator, TPayload> me)
		where TDiscriminator : notnull
		where TPayload : notnull =>
		new PodBuilder2<TDiscriminator, TPayload, JsonPod2<TDiscriminator, TPayload>>(me);
	
	public static IPodBuilder2<JsonPod2<TDiscriminator, object>> FromJson<TDiscriminator>(this IPodBuilder2<IPod2<TDiscriminator, string>> me) 
		where TDiscriminator : notnull
	{
		// TODO remove nullable Payload!
		var pod = me.Pod.Payload!.FromJsonPod2<TDiscriminator>();
		return new PodBuilder2<TDiscriminator, object, JsonPod2<TDiscriminator, object>>(pod);
	}

	public static IPodBuilder2<IPod2<TDiscriminator, byte[]>> FromBytes<TDiscriminator, T>(this IPodBuilder2<JsonPod2<TDiscriminator, T>> me) where TDiscriminator : notnull =>
		new PodBuilder2<TDiscriminator, byte[], IPod2<TDiscriminator, byte[]>>(new Pod2<TDiscriminator, byte[]>(me.Pod.Discriminator,
			me.Pod.As<byte[]>() ?? throw new ArgumentException($"'{me.GetType().GetSignature()}' payload couldn't be deserialized as '{typeof(byte[]).GetSignature()}'")));
	
}