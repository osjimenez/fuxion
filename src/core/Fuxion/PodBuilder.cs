namespace Fuxion;

public interface IPodPreBuilder<out TPayload>
{
	TPayload Payload { get; }
	
}

public interface IPodBuilder<out TDiscriminator, out TPayload, out TPod>
	where TPod : IPod<TDiscriminator, TPayload>
{
	TPod Pod { get; }
}

class PodPreBuilder<TPayload>(TPayload payload) : IPodPreBuilder<TPayload>
	where TPayload : notnull
{
	public TPayload Payload { get; } = payload;
}

class PodBuilder<TDiscriminator, TPayload, TPod>(TPod pod) : IPodBuilder<TDiscriminator, TPayload, TPod>
	where TDiscriminator : notnull
	where TPayload : notnull
	where TPod : IPod<TDiscriminator, TPayload>
{
	public TPod Pod { get; set; } = pod;
}