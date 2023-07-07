using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fuxion.Text.Json.Serialization;

namespace Fuxion.Text.Json;

public static class JsonNodePodExtensions
{
	public static IPodBuilder<TDiscriminator, JsonNode, JsonNodePod<TDiscriminator>> ToJsonNode<TDiscriminator, TPayload>(this IPodBuilder<TDiscriminator, TPayload,IPod<TDiscriminator, TPayload>> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		where TPayload : notnull
		=> new PodBuilder<TDiscriminator, JsonNode, JsonNodePod<TDiscriminator>>(new(discriminator, me.Pod));
	public static IPodBuilder<TDiscriminator, JsonNode, JsonNodePod<TDiscriminator>> ToJsonNode<TDiscriminator, TPayload>(this IPodPreBuilder<TPayload> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		where TPayload : notnull
		=> new PodBuilder<TDiscriminator, JsonNode, JsonNodePod<TDiscriminator>>(new(discriminator, me.Payload));
	public static IPodBuilder<TDiscriminator, JsonNode, JsonNodePod<TDiscriminator>> FromJsonNode<TDiscriminator>(this IPodBuilder<TDiscriminator, string,IPod<TDiscriminator, string>> me)
		where TDiscriminator : notnull
	{
		JsonSerializerOptions options = new();
		options.Converters.Add(new IPodConverterFactory());
		return new PodBuilder<TDiscriminator, JsonNode, JsonNodePod<TDiscriminator>>(me.Pod.Payload.DeserializeFromJson<JsonNodePod<TDiscriminator>>(options: options)
			?? throw new SerializationException("string couldn't be deserialized"));
	}
	public static IPodBuilder<TDiscriminator, JsonNode, JsonNodePod<TDiscriminator>> FromJsonNode<TDiscriminator>(this IPodPreBuilder<string> me)
		where TDiscriminator : notnull
		=> new PodBuilder<TDiscriminator, JsonNode, JsonNodePod<TDiscriminator>>(me.Payload.DeserializeFromJson<JsonNodePod<TDiscriminator>>()
			?? throw new SerializationException("string couldn't be deserialized"));
	public static IPodBuilder<TDiscriminator, JsonNode, JsonNodePod<TDiscriminator>> FromJsonNode<TDiscriminator>(this IPodPreBuilder<string> me, out JsonNodePod<TDiscriminator> pod)
		where TDiscriminator : notnull
	{
		var deserializedPod = me.Payload.DeserializeFromJson<JsonNodePod<TDiscriminator>>() ?? throw new SerializationException("string couldn't be deserialized");
		pod = deserializedPod;
		return new PodBuilder<TDiscriminator, JsonNode, JsonNodePod<TDiscriminator>>(deserializedPod);
	}
	public static IPodBuilder<TDiscriminator, byte[], IPod<TDiscriminator, byte[]>> ToUtf8Bytes<TDiscriminator>(this IPodBuilder<TDiscriminator, JsonNode, JsonNodePod<TDiscriminator>> me, TDiscriminator discriminator)
		where TDiscriminator : notnull
		=> new PodBuilder<TDiscriminator, byte[], IPod<TDiscriminator, byte[]>>(new Pod<TDiscriminator, byte[]>(discriminator, Encoding.UTF8.GetBytes(me.Pod.Payload.ToJsonString())));
}