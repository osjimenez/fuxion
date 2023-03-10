using System.Text.Json;
using System.Text.Json.Serialization;
using Fuxion.Reflection;

namespace Fuxion.Json;

// [JsonConverter(typeof(TypeKeyPodConverterFactory))]
public class TypeKeyPod<TPayload> : JsonPod<TypeKey, TPayload>
	where TPayload : notnull
{
	[JsonConstructor]
	protected TypeKeyPod() { }
	public TypeKeyPod(TPayload payload) : base(payload.GetType().GetTypeKey(), payload) { }
}
// public class TypeKeyPodJsonConverter<TPayload> : JsonPodConverter<TypeKeyPod<TPayload>, TypeKey, TPayload> where TPayload : notnull 
// { }
// public class TypeKeyPodConverterFactory : JsonConverterFactory
// {
// 	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsSubclassOfRawGeneric(typeof(TypeKeyPod<>));
// 	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
// 	{
// 		var types = typeToConvert.GetGenericArguments();
// 		var converterType = typeof(TypeKeyPodJsonConverter<>).MakeGenericType(types);
// 		return (JsonConverter)(Activator.CreateInstance(converterType) ?? throw new InvalidCastException($"'{converterType.GetSignature()}' can not be created"));
// 	}
// }
//
//
//
//
// public static class JsonPodCollectionTypeKeyExtensions
// {
// 	// public void Add<TPayload>(TDiscriminator discriminator, TPayload payload) where TPayload : notnull => Add<TPayload>(new(discriminator, payload));
// 	public static void Add<TPayload>(this JsonPodCollection<TypeKey> me, TPayload payload)
// 		where TPayload : notnull
// 	{
// 		me.Add<TPayload>(payload.GetType().GetTypeKey(), payload);
// 	}
// 	public static bool Has<TPayload>(this JsonPodCollection<TypeKey> me) => me.InternalDictionary.ContainsKey(typeof(TPayload).GetTypeKey());
// 	public static JsonPod<TypeKey, TPayload>? GetPod<TPayload>(this JsonPodCollection<TypeKey> me)
// 		//where TPayload : notnull
// 		=> me.InternalDictionary[typeof(TPayload).GetTypeKey()].Deserialize<JsonPod<TypeKey, TPayload>>();
//
// 	public static TPayload? WithTypeKeyResolver<TPayload>(this JsonPod<TypeKey, TPayload> me, ITypeKeyResolver typeKeyResolver, JsonSerializerOptions? options = null) 
// 		//where TPayload : notnull
// 		=> (TPayload)me.As(typeKeyResolver[typeof(TPayload).GetTypeKey()], options);
// }