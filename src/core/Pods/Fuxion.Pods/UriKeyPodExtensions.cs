using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fuxion.Pods.Json;
using Fuxion.Pods.Json.Serialization;

namespace Fuxion.Pods;

public static class UriKeyPodExtensions
{
	public static IUriKeyPodPreBuilder<TPayload> BuildUriKeyPod<TPayload>(this TPayload me, IUriKeyResolver resolver)
		where TPayload : notnull
		=> new UriKeyPodPreBuilder<TPayload>(resolver, me);
	public static IUriKeyPodBuilder<TPayload, TPod> RebuildUriKeyPod<TPayload, TPod>(this TPod me)
		where TPayload : notnull
		where TPod : IUriKeyPod<TPayload>
		=> new UriKeyPodBuilder<TPayload, TPod>(me.Resolver ?? throw new UriKeyResolverNotFoundException($"The pod '{me.GetType().GetSignature()}' hasn't '{nameof(me.Resolver)}' and cannot rebuild"),
			me);
	public static IUriKeyPodBuilder<TPayload, IUriKeyPod<TPayload>> ToUriKeyPod<TPayload>(this IUriKeyPodPreBuilder<TPayload> me)
		where TPayload : notnull
		=> new UriKeyPodBuilder<TPayload, IUriKeyPod<TPayload>>(me.Resolver, new UriKeyPod<TPayload>(me.Resolver[me.Payload.GetType()], me.Payload)
		{
			Resolver = me.Resolver
		});
	public static TPodBuilder AddUriKeyHeader<TPodBuilder>(this TPodBuilder me, object payload)
		where TPodBuilder : IUriKeyPodBuilder<object, ICollectionPod<UriKey, object>>
	{
		me.Pod.Add(new UriKeyPod<object>(me.Resolver[payload.GetType()], payload));
		return me;
	}
	// public static IUriKeyPodBuilder<JsonNode, JsonNodePod<UriKey>> ToJsonNode<TPayload>(this IUriKeyPodPreBuilder<TPayload> me)
	// 	where TPayload : notnull
	// 	=> new UriKeyPodBuilder<JsonNode, JsonNodePod<UriKey>>(me.Resolver, new(me.Resolver[typeof(JsonNode)], me.Pod, me.Resolver));
	
	// TODO: - Alonso - Implement this in other way:
	public static IUriKeyPodBuilder<JsonNode, JsonNodePod<UriKey>> ToJsonNode<TPayload>(this IUriKeyPodBuilder<TPayload, IPod<UriKey, TPayload>> me)
		where TPayload : notnull
		=> new UriKeyPodBuilder<JsonNode, JsonNodePod<UriKey>>(me.Resolver, new(me.Resolver[typeof(JsonNode)], me.Pod, me.Resolver));
	public static IUriKeyPodBuilder<object, IUriKeyPod<object>> FromJsonNode(this IUriKeyPodBuilder<string, IPod<UriKey, string>> me)
	{
		JsonSerializerOptions options = new();
		options.PropertyNameCaseInsensitive = true;
		options.Converters.Add(new IPodConverterFactory(me.Resolver));
		return new UriKeyPodBuilder<object, IUriKeyPod<object>>(me.Resolver,
			me.Pod.Payload.DeserializeFromJson<UriKeyPod<object>>(options: options) ?? throw new SerializationException("string couldn't be deserialized"));
	}
	public static IUriKeyPodBuilder<object, IUriKeyPod<object>> FromJsonNode(this IUriKeyPodPreBuilder<string> me)
	{
		JsonSerializerOptions options = new();
		options.PropertyNameCaseInsensitive = true;
		options.Converters.Add(new IPodConverterFactory(me.Resolver));
		return new UriKeyPodBuilder<object, IUriKeyPod<object>>(me.Resolver,
			me.Payload.DeserializeFromJson<UriKeyPod<object>>(options: options) ?? throw new SerializationException("string couldn't be deserialized"));
	}
	public static IUriKeyPodBuilder<object, IUriKeyPod<object>> FromJsonNode(this IUriKeyPodPreBuilder<string> me, out IUriKeyPod<object> pod)
	{
		JsonSerializerOptions options = new();
		options.PropertyNameCaseInsensitive = true;
		options.Converters.Add(new IPodConverterFactory(me.Resolver));
		var deserializedPod = me.Payload.DeserializeFromJson<UriKeyPod<object>>(options: options) ?? throw new SerializationException("string couldn't be deserialized");
		pod = deserializedPod;
		return new UriKeyPodBuilder<object, IUriKeyPod<object>>(me.Resolver, deserializedPod);
	}
	public static IUriKeyPodBuilder<byte[], IUriKeyPod<byte[]>> ToUtf8Bytes(this IUriKeyPodBuilder<string, IPod<UriKey, string>> me)
		=> new UriKeyPodBuilder<byte[], IUriKeyPod<byte[]>>(me.Resolver, new UriKeyPod<byte[]>(me.Resolver[typeof(byte[])], Encoding.UTF8.GetBytes(me.Pod.Payload)));
	public static IUriKeyPodBuilder<byte[], IUriKeyPod<byte[]>> ToUtf8Bytes(this IUriKeyPodBuilder<JsonNode, IPod<UriKey, JsonNode>> me)
		=> new UriKeyPodBuilder<byte[], IUriKeyPod<byte[]>>(me.Resolver, new UriKeyPod<byte[]>(me.Resolver[typeof(byte[])], Encoding.UTF8.GetBytes(me.Pod.Payload.ToJsonString())));
	public static IUriKeyPodBuilder<string, IUriKeyPod<string>> FromUtf8Bytes(this IUriKeyPodPreBuilder<byte[]> me)
		=> new UriKeyPodBuilder<string, IUriKeyPod<string>>(me.Resolver, new UriKeyPod<string>(me.Resolver[typeof(string)], Encoding.UTF8.GetString(me.Payload)));
}