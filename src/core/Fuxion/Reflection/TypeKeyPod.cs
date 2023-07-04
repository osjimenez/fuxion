// using System.Runtime;
// using System.Text.Json.Serialization;
//
// namespace Fuxion.Reflection;
//
//
// public interface ITypeKeyPod<out TPayload> : ICollectionPod<TypeKey, TPayload>
// {
// 	
// }
//
// public class TypeKeyPod<TPayload>(TypeKey discriminator, TPayload payload) : Pod<TypeKey, TPayload>(discriminator, payload), ITypeKeyPod<TPayload>
// {
// 	// ATTENTION: This constructor cannot be removed, it is needed for deserialization
// 	protected TypeKeyPod():this(default!,default!){}
// }