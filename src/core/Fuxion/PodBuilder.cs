namespace Fuxion;

public interface IPodPreBuilder<out TPayload>
{
	TPayload Payload { get; }
	
}

public interface IPodBuilder<out TPod>
{
	TPod Pod { get; }
}

class PodPreBuilder<TPayload>(TPayload payload) : IPodPreBuilder<TPayload>
	where TPayload : notnull
{
	public TPayload Payload { get; } = payload;
	public void AddHeader<TDiscriminator>(IPod<TDiscriminator, object> pod)
	{
		
	}
}

class PodBuilder<TDiscriminator, TPayload, TPod>(TPod pod) : IPodBuilder<TPod>
	where TDiscriminator : notnull
	where TPayload : notnull
	where TPod : IPod<TDiscriminator, TPayload>
{
	public TPod Pod { get; } = pod;
}