using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fuxion.Text.Json;
using Fuxion.Text.Json.Serialization;

namespace Fuxion.Reflection;

public static class TypeKeyPodExtensions
{
	public static ITypeKeyPodPreBuilder<TPayload> BuildTypeKeyPod<TPayload>(this TPayload me, ITypeKeyResolver resolver)
		where TPayload : notnull
		=> new TypeKeyPodPreBuilder<TPayload>(resolver, me);
	public static ITypeKeyPodBuilder<TPayload, ITypeKeyPod<TPayload>> ToTypeKeyPod<TPayload>(this ITypeKeyPodPreBuilder<TPayload> me)
		where TPayload : notnull
		=> new TypeKeyPodBuilder<TPayload, TypeKeyPod<TPayload>>(me.Resolver, new(me.Resolver[me.Payload.GetType()], me.Payload));
	public static TPodBuilder AddTypeKeyHeader<TPodBuilder>(this TPodBuilder me, object payload)
		where TPodBuilder : ITypeKeyPodBuilder<object, ICollectionPod<TypeKey, object>>
	{
		me.Pod.Add(new TypeKeyPod<object>(me.Resolver[payload.GetType()], payload));
		return me;
	}
	public static ITypeKeyPodBuilder<JsonNode, JsonNodePod<TypeKey>> ToJsonNode<TPayload>(this ITypeKeyPodBuilder<TPayload, IPod<TypeKey, TPayload>> me)
		where TPayload : notnull
		=> new TypeKeyPodBuilder<JsonNode, JsonNodePod<TypeKey>>(me.Resolver, new(me.Resolver[typeof(JsonNode)], me.Pod, me.Resolver));
	public static ITypeKeyPodBuilder<object, TypeKeyPod<object>> FromJsonNode(this ITypeKeyPodBuilder<string,IPod<TypeKey, string>> me)
	{
		JsonSerializerOptions options = new();
		options.Converters.Add(new IPodConverterFactory(me.Resolver));
		return new TypeKeyPodBuilder<object, TypeKeyPod<object>>(me.Resolver, me.Pod.Payload.DeserializeFromJson<TypeKeyPod<object>>(options: options)
			?? throw new SerializationException("string couldn't be deserialized"));
	}
	
	public static ITypeKeyPodBuilder<byte[], ITypeKeyPod<byte[]>> ToUtf8Bytes(this ITypeKeyPodBuilder<string, IPod<TypeKey, string>> me) 
		=> new TypeKeyPodBuilder<byte[], ITypeKeyPod<byte[]>>(me.Resolver, new TypeKeyPod<byte[]>(me.Resolver[typeof(byte[])], Encoding.UTF8.GetBytes(me.Pod.Payload)));
	public static ITypeKeyPodBuilder<byte[], ITypeKeyPod<byte[]>> ToUtf8Bytes(this ITypeKeyPodBuilder<JsonNode, IPod<TypeKey, JsonNode>> me) 
		=> new TypeKeyPodBuilder<byte[], ITypeKeyPod<byte[]>>(me.Resolver, new TypeKeyPod<byte[]>(me.Resolver[typeof(byte[])], Encoding.UTF8.GetBytes(me.Pod.Payload.ToJsonString())));
	public static ITypeKeyPodBuilder<string, ITypeKeyPod<string>> FromUtf8Bytes(this ITypeKeyPodPreBuilder<byte[]> me)
		=> new TypeKeyPodBuilder<string, ITypeKeyPod<string>>(me.Resolver, new TypeKeyPod<string>(me.Resolver[typeof(string)], Encoding.UTF8.GetString(me.Payload)));
	public static T? Ass<T>(this JsonNodePod<TypeKey> me, ITypeKeyResolver resolver)
	{
		JsonSerializerOptions options = new();
		options.Converters.Add(new IPodConverterFactory(resolver));
		var res = me.Payload.Deserialize<T>(options);
		return res ?? default;
	}
}