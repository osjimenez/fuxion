using Fuxion.Pods;

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

public class PodPreBuilder<TPayload>(TPayload payload) : IPodPreBuilder<TPayload>
	where TPayload : notnull
{
	public TPayload Payload { get; } = payload;
}

public class PodBuilder<TDiscriminator, TPayload, TPod>(TPod pod) : IPodBuilder<TDiscriminator, TPayload, TPod>
	where TDiscriminator : notnull
	where TPayload : notnull
	where TPod : IPod<TDiscriminator, TPayload>
{
	public TPod Pod { get; set; } = pod;
}