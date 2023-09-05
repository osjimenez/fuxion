// using Fuxion.Application.Snapshots;
// using Fuxion.Domain;
// using Fuxion.Domain.Aggregates;
//
// namespace Fuxion.Application.Aggregates;
//
// public static class EventSourcingAggregateFeatureExtensions
// {
// 	public static bool HasEventSourcing(this Aggregate me) => me.HasFeature<EventSourcingAggregateFeature>();
// 	public static EventSourcingAggregateFeature EventSourcing(this Aggregate me) => me.GetFeature<EventSourcingAggregateFeature>();
// 	public static void AttachEventSourcing(this Aggregate me) => me.AttachFeature(new EventSourcingAggregateFeature());
// 	public static void Hydrate(this Aggregate me, IEnumerable<Event> events) => me.EventSourcing().Hydrate(events);
// 	public static int GetLastCommittedVersion(this Aggregate me) => me.EventSourcing().LastCommittedVersion;
// 	public static int GetCurrentVersion(this Aggregate me) => me.EventSourcing().CurrentVersion;
// 	public static bool IsSnapshottable(this Aggregate me) => me.EventSourcing().IsSnapshottable;
// 	public static int GetSnapshotVersion(this Aggregate me) => me.EventSourcing().SnapshotVersion;
// 	public static int GetSnapshotFrequency(this Aggregate me) => me.EventSourcing().SnapshotFrequency;
// 	public static Snapshot GetSnapshot(this Aggregate me) => me.EventSourcing().GetSnapshot();
// }