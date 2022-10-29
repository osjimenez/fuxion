using Fuxion.Application.Aggregates;
using Fuxion.Domain;

namespace Fuxion.Application.Factories;

public static class SnapshotFactoryFeatureExtensions
{
	public static bool IsSnapshottable<TAggregate>(this Factory<TAggregate> me) where TAggregate : Aggregate, new() => me.HasFeature<TAggregate, SnapshotFactoryFeature<TAggregate>>();
	public static Type GetSnapshotType<TAggregate>(this Factory<TAggregate> me) where TAggregate : Aggregate, new() => me.GetFeature<TAggregate, SnapshotFactoryFeature<TAggregate>>().Type;
	public static TAggregate FromSnapshot<TAggregate>(this Factory<TAggregate> me, Snapshot<TAggregate> snapshot) where TAggregate : Aggregate, new()
	{
		var agg = me.Create(snapshot.AggregateId);
		snapshot.Hydrate(agg);
		agg.EventSourcing().CurrentVersion       = snapshot.Version;
		agg.EventSourcing().LastCommittedVersion = snapshot.Version;
		agg.EventSourcing().SnapshotVersion      = snapshot.Version;
		return agg;
	}
}