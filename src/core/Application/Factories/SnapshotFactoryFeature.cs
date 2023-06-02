using Fuxion.Application.Aggregates;
using Fuxion.Domain;

namespace Fuxion.Application.Factories;

public class SnapshotFactoryFeature<TAggregate> : IFactoryFeature<TAggregate> where TAggregate : IAggregate, new()
{
	public SnapshotFactoryFeature(Type type, int frecuency)
	{
		Type = type;
		Frecuency = frecuency;
	}
	internal int Frecuency { get; set; }
	internal Type Type { get; set; }
	Factory<TAggregate>? IFactoryFeature<TAggregate>.Factory { get; set; }
	public void Initialize(TAggregate aggregate)
	{
		aggregate.Features().Get<EventSourcingAggregateFeature>().SnapshotType = Type;
		aggregate.Features().Get<EventSourcingAggregateFeature>().SnapshotFrequency = Frecuency;
		// aggregate.EventSourcing().SnapshotType = Type;
		// aggregate.EventSourcing().SnapshotFrequency = Frecuency;
	}
	public TAggregate FromSnapshot(Snapshot<TAggregate> snapshot)
	{
		var agg = ((IFactoryFeature<TAggregate>)this).Create(snapshot.AggregateId);
		snapshot.Hydrate(agg);
		var feature = agg.Features().Get<EventSourcingAggregateFeature>();
		feature.CurrentVersion = snapshot.Version;
		feature.LastCommittedVersion = snapshot.Version;
		feature.SnapshotVersion = snapshot.Version;
		return agg;
	}
}