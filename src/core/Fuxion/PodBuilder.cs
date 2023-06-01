namespace Fuxion;

public interface IPodBuilder<out TPod>
{
	TPod Pod { get; }
}

class PodBuilder<TDiscriminator, TOutside, TInside, TPod> : IPodBuilder<TPod> where TDiscriminator : notnull
	where TOutside : notnull
	where TInside : notnull
	where TPod : ICrossPod<TDiscriminator, TOutside, TInside>
{
	public PodBuilder(TPod pod)
	{
		Pod = pod;
	}
	public TPod Pod { get; }
}