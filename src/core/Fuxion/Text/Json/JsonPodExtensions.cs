using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Nodes;

namespace Fuxion.Text.Json;

public static class JsonPod2Extensions
{
	public static IPodBuilder<JsonNodePod<TDiscriminator>> ToJsonNode<TDiscriminator, TPayload>(this IPodBuilder<IPod<TDiscriminator, TPayload>> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		where TPayload : notnull
		=> new PodBuilder<TDiscriminator, JsonNode, JsonNodePod<TDiscriminator>>(new(discriminator, me.Pod));
	public static IPodBuilder<JsonNodePod<TDiscriminator>> ToJsonNode<TDiscriminator, TPayload>(this IPodPreBuilder<TPayload> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		where TPayload : notnull
		=> new PodBuilder<TDiscriminator, JsonNode, JsonNodePod<TDiscriminator>>(new(discriminator, me.Payload));
	public static IPodBuilder<IPod<TDiscriminator, byte[]>> ToUtf8Bytes<TDiscriminator>(this IPodBuilder<IPod<TDiscriminator, string>> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		=> new PodBuilder<TDiscriminator, byte[], IPod<TDiscriminator, byte[]>>(new Pod<TDiscriminator, byte[]>(discriminator, Encoding.UTF8.GetBytes(me.Pod.Payload)));
	public static IPodBuilder<IPod<TDiscriminator, string>> FromUtf8Bytes<TDiscriminator>(this IPodBuilder<IPod<TDiscriminator, byte[]>> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		=> new PodBuilder<TDiscriminator, string, IPod<TDiscriminator, string>>(new Pod<TDiscriminator, string>(discriminator, Encoding.UTF8.GetString(me.Pod.Payload)));
	public static IPodBuilder<IPod<TDiscriminator, string>> FromUtf8Bytes<TDiscriminator>(this IPodPreBuilder<byte[]> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		=> new PodBuilder<TDiscriminator, string, IPod<TDiscriminator, string>>(new Pod<TDiscriminator, string>(discriminator, Encoding.UTF8.GetString(me.Payload)));
	public static IPodBuilder<JsonNodePod<TDiscriminator>> FromJsonNode<TDiscriminator>(this IPodBuilder<IPod<TDiscriminator, string>> me)
		where TDiscriminator : notnull
		=> new PodBuilder<TDiscriminator, JsonNode, JsonNodePod<TDiscriminator>>(me.Pod.Payload.DeserializeFromJson<JsonNodePod<TDiscriminator>>()
			?? throw new SerializationException("string couldn't be deserialized"));
	public static IPodBuilder<JsonNodePod<TDiscriminator>> FromJson<TDiscriminator>(this IPodPreBuilder<string> me)
		where TDiscriminator : notnull
		=> new PodBuilder<TDiscriminator, JsonNode, JsonNodePod<TDiscriminator>>(me.Payload.DeserializeFromJson<JsonNodePod<TDiscriminator>>()
			?? throw new SerializationException("string couldn't be deserialized"));
}