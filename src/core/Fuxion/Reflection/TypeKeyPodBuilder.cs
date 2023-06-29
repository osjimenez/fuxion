namespace Fuxion.Reflection;

public interface ITypeKeyPodPreBuilder<out TPayload> : IPodPreBuilder<TPayload>
{
	ITypeKeyResolver Resolver { get; }
}
public interface ITypeKeyPodBuilder<out TPayload, out TPod> : IPodBuilder<TypeKey, TPayload, TPod>
	where TPod : IPod<TypeKey, TPayload>
{
	ITypeKeyResolver Resolver { get; }
}
class TypeKeyPodPreBuilder<TPayload>(ITypeKeyResolver resolver,TPayload payload) : ITypeKeyPodPreBuilder<TPayload>
	where TPayload : notnull
{
	public TPayload Payload { get; } = payload;
	public ITypeKeyResolver Resolver { get; } = resolver;
}
class TypeKeyPodBuilder<TPayload, TPod>(ITypeKeyResolver resolver, TPod pod) : ITypeKeyPodBuilder<TPayload, TPod>
	where TPayload : notnull
	where TPod : IPod<TypeKey, TPayload>
{
	public TPod Pod { get; } = pod;
	public ITypeKeyResolver Resolver { get; } = resolver;
}