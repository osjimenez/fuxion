using Fuxion.Application.Events;
using Fuxion.Application.Snapshots;
using Fuxion.Domain;
using Fuxion.Domain.Aggregates;

namespace Fuxion.Application.Aggregates;

public class EventSourcingAggregateFeature : IFeature<IAggregate>
{
	IAggregate? aggregate;
	// Versioning
	public int LastCommittedVersion { get; internal set; }
	public int CurrentVersion { get; internal set; }
	// Snapshoting
	public bool IsSnapshottable => SnapshotType != null;
	public Type? SnapshotType { get; internal set; }
	public int SnapshotFrequency { get; internal set; }
	public int SnapshotVersion { get; internal set; }
	public void OnAttach(IAggregate aggregate)
	{
		this.aggregate = aggregate;
		if (!aggregate.Features().Has<EventsAggregateFeature>()) throw new FeatureNotFoundException($"{nameof(EventSourcingAggregateFeature)} requires {nameof(EventsAggregateFeature)}");
		aggregate.Features().Get<EventsAggregateFeature>().Applying += (s, e) =>
		{
			if (!e.Value.Features().Has<EventSourcingEventFeature>())
				e.Value.Features().Add<EventSourcingEventFeature>(f =>
				{
					f.TargetVersion = CurrentVersion;
					f.CorrelationId = Guid.NewGuid();
					f.EventCommittedTimestamp = DateTime.UtcNow;
					f.ClassVersion = 0;
				});
		};
		aggregate.Features().Get<EventsAggregateFeature>().Validated += (s, e) => {
			if (CurrentVersion != e.Value.Features().Get<EventSourcingEventFeature>().TargetVersion)
				throw new AggregateStateMismatchException($"Aggregate '{aggregate.GetType().Name}' has version '{CurrentVersion}' and event has target version '{e.Value.Features().Get<EventSourcingEventFeature>().TargetVersion}'");
		};
		aggregate.Features().Get<EventsAggregateFeature>().Pendent += (s, e) => CurrentVersion++;
	}
	// Hydrating
	internal void Hydrate(IEnumerable<Event> events)
	{
		var feature = aggregate?.Features().Get<EventsAggregateFeature>();
		foreach (var @event in events)
		{
			var evtFeature = @event.Features().Get<EventSourcingEventFeature>();
			evtFeature.IsReplay = true;
			feature?.ApplyEvent(@event);
		}
		feature?.ClearPendingEvents();
		LastCommittedVersion = CurrentVersion;
	}
	public Snapshot GetSnapshot()
	{
		if (SnapshotType == null) throw new AggregateIsNotSnapshottableException();
		if (aggregate == null) throw new InvalidStateException($"'{nameof(GetSnapshot)}' was called before this '{nameof(EventSourcingAggregateFeature)}' would be attached to an aggregate");
		var snap = (Snapshot?)Activator.CreateInstance(SnapshotType);
		if (snap == null) throw new InvalidProgramException("Instance of snapshot cannot be created");
		snap.AggregateId = aggregate.Id;
		snap.Version = aggregate.Get<EventSourcingAggregateFeature>().CurrentVersion;
		snap.Load(aggregate);
		return snap;
	}
}