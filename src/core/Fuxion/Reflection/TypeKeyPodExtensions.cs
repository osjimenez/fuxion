using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fuxion.Text.Json;

namespace Fuxion.Reflection;

public static class TypeKeyPodExtensions
{
	public static ITypeKeyPodPreBuilder<TPayload> BuildTypeKeyPod<TPayload>(this TPayload me, ITypeKeyResolver resolver)
		where TPayload : notnull
		=> new TypeKeyPodPreBuilder<TPayload>(resolver, me);
	public static ITypeKeyPodBuilder<ITypeKeyPod<TPayload>> ToTypeKeyPod<TPayload>(this ITypeKeyPodPreBuilder<TPayload> me)
		where TPayload : notnull
		=> new TypeKeyPodBuilder<TPayload, TypeKeyPod<TPayload>>(me.Resolver, new(me.Resolver[me.Payload.GetType()], me.Payload));
	public static TPodBuilder AddTypeKeyHeader<TPodBuilder>(this TPodBuilder me, object payload)
		where TPodBuilder : ITypeKeyPodBuilder<ICollectionPod<TypeKey, object>>
	{
		me.Pod.Add(new Pod<TypeKey, object>(me.Resolver[payload.GetType()], payload));
		return me;
	}
	public static ITypeKeyPodBuilder<JsonNodePod<TypeKey>> ToJsonNode<TPayload>(this ITypeKeyPodBuilder<IPod<TypeKey, TPayload>> me)
		where TPayload : notnull
		=> new TypeKeyPodBuilder<JsonNode, JsonNodePod<TypeKey>>(me.Resolver, new(me.Resolver[typeof(JsonNode)], me.Pod));
	
	// public static ITypeKeyPodBuilder<ITypeKeyPod<byte[]>> ToUtf8Bytes(this ITypeKeyPodBuilder<string> me)
	// 	=> new TypeKeyPodBuilder<byte[], ITypeKeyPod<byte[]>>(me.Resolver, new TypeKeyPod<byte[]>(discriminator, Encoding.UTF8.GetBytes(me.Pod.Payload)));
	public static ITypeKeyPodBuilder<ITypeKeyPod<string>> FromUtf8Bytes(this ITypeKeyPodPreBuilder<byte[]> me)
		=> new TypeKeyPodBuilder<string, ITypeKeyPod<string>>(me.Resolver, new TypeKeyPod<string>(me.Resolver[typeof(string)], Encoding.UTF8.GetString(me.Payload)));
	public static T? Ass<T>(this JsonNodePod<TypeKey> me)
	{
		JsonSerializerOptions options = new();
		options.Converters.Add(new IPodConverterFactory());
		var res = me.Payload.Deserialize<T>(options);
		return res ?? default;
	}
}