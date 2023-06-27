namespace Fuxion;

public interface IPodPreBuilder2<out TPayload>
{
	TPayload Payload { get; }
}
public interface IPodBuilder2<out TPod>
{
	TPod Pod { get; }
}
class PodPreBuilder2<TPayload>(TPayload payload) : IPodPreBuilder2<TPayload> 
	where TPayload : notnull
{
	public TPayload Payload { get; } = payload;
}
class PodBuilder2<TDiscriminator, TPayload, TPod>(TPod pod) : IPodBuilder2<TPod> 
	where TDiscriminator : notnull
	where TPayload : notnull
	where TPod : IPod2<TDiscriminator, TPayload>
{
	public TPod Pod { get; } = pod;
}