namespace Fuxion;

public interface IUriKeyPodPreBuilder<out TPayload> : IPodPreBuilder<TPayload>
{
	IUriKeyResolver Resolver { get; }
}
public interface IUriKeyPodBuilder<out TPayload, out TPod> : IPodBuilder<UriKey, TPayload, TPod>
	where TPod : IPod<UriKey, TPayload>
{
	IUriKeyResolver Resolver { get; }
}
class UriKeyPodPreBuilder<TPayload>(IUriKeyResolver resolver,TPayload payload) : IUriKeyPodPreBuilder<TPayload>
	where TPayload : notnull
{
	public TPayload Payload { get; } = payload;
	public IUriKeyResolver Resolver { get; } = resolver;
}
class UriKeyPodBuilder<TPayload, TPod>(IUriKeyResolver resolver, TPod pod) : IUriKeyPodBuilder<TPayload, TPod>
	where TPayload : notnull
	where TPod : IPod<UriKey, TPayload>
{
	public TPod Pod { get; } = pod;
	public IUriKeyResolver Resolver { get; } = resolver;
}