namespace Fuxion;

public interface IPodBuilder2<out TPod>
{
	TPod Pod { get; }
}

class PodBuilder2<TDiscriminator, TPayload, TPod> : IPodBuilder2<TPod> where TDiscriminator : notnull
	where TPayload : notnull
	where TPod : IPod2<TDiscriminator, TPayload>
{
	public PodBuilder2(TPod pod)
	{
		Pod = pod;
	}
	public TPod Pod { get; }
}