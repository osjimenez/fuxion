namespace Fuxion.Application;

using Fuxion.Application.Snapshots;
using Fuxion.Domain;

public abstract class Snapshot<TAggregate> : Snapshot where TAggregate : Aggregate, new()
{
	protected internal abstract void Hydrate(TAggregate aggregate);
	internal override void Load(Aggregate aggregate) => Load((TAggregate)aggregate);
	protected internal abstract void Load(TAggregate aggregate);
}