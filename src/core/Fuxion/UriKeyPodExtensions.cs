using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fuxion.Text.Json;
using Fuxion.Text.Json.Serialization;

namespace Fuxion;

public static class UriKeyPodExtensions
{
	public static IUriKeyPodPreBuilder<TPayload> BuildUriKeyPod<TPayload>(this TPayload me, IUriKeyResolver resolver)
		where TPayload : notnull
		=> new UriKeyPodPreBuilder<TPayload>(resolver, me);
	public static IUriKeyPodBuilder<TPayload, IUriKeyPod<TPayload>> ToUriKeyPod<TPayload>(this IUriKeyPodPreBuilder<TPayload> me)
		where TPayload : notnull
		=> new UriKeyPodBuilder<TPayload, IUriKeyPod<TPayload>>(me.Resolver, new UriKeyPod<TPayload>(me.Resolver[me.Payload.GetType()], me.Payload));
	public static TPodBuilder AddUriKeyHeader<TPodBuilder>(this TPodBuilder me, object payload)
		where TPodBuilder : IUriKeyPodBuilder<object, ICollectionPod<UriKey, object>>
	{
		me.Pod.Add(new UriKeyPod<object>(me.Resolver[payload.GetType()], payload));
		return me;
	}
	public static IUriKeyPodBuilder<JsonNode, JsonNodePod<UriKey>> ToJsonNode<TPayload>(this IUriKeyPodBuilder<TPayload, IPod<UriKey, TPayload>> me)
		where TPayload : notnull
		=> new UriKeyPodBuilder<JsonNode, JsonNodePod<UriKey>>(me.Resolver, new(me.Resolver[typeof(JsonNode)], me.Pod, me.Resolver));
	public static IUriKeyPodBuilder<object, IUriKeyPod<object>> FromJsonNode(this IUriKeyPodBuilder<string,IPod<UriKey, string>> me)
	{
		JsonSerializerOptions options = new();
		options.Converters.Add(new IPodConverterFactory(me.Resolver));
		return new UriKeyPodBuilder<object, IUriKeyPod<object>>(me.Resolver, me.Pod.Payload.DeserializeFromJson<UriKeyPod<object>>(options: options)
			?? throw new SerializationException("string couldn't be deserialized"));
	}
	
	public static IUriKeyPodBuilder<byte[], IUriKeyPod<byte[]>> ToUtf8Bytes(this IUriKeyPodBuilder<string, IPod<UriKey, string>> me) 
		=> new UriKeyPodBuilder<byte[], IUriKeyPod<byte[]>>(me.Resolver, new UriKeyPod<byte[]>(me.Resolver[typeof(byte[])], Encoding.UTF8.GetBytes(me.Pod.Payload)));
	public static IUriKeyPodBuilder<byte[], IUriKeyPod<byte[]>> ToUtf8Bytes(this IUriKeyPodBuilder<JsonNode, IPod<UriKey, JsonNode>> me) 
		=> new UriKeyPodBuilder<byte[], IUriKeyPod<byte[]>>(me.Resolver, new UriKeyPod<byte[]>(me.Resolver[typeof(byte[])], Encoding.UTF8.GetBytes(me.Pod.Payload.ToJsonString())));
	public static IUriKeyPodBuilder<string, IUriKeyPod<string>> FromUtf8Bytes(this IUriKeyPodPreBuilder<byte[]> me)
		=> new UriKeyPodBuilder<string, IUriKeyPod<string>>(me.Resolver, new UriKeyPod<string>(me.Resolver[typeof(string)], Encoding.UTF8.GetString(me.Payload)));
}