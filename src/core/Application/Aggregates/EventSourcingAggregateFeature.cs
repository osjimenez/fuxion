using Fuxion.Application.Events;
using Fuxion.Application.Snapshots;
using Fuxion.Domain;
using Fuxion.Domain.Aggregates;
using Fuxion.Domain.Events;

namespace Fuxion.Application.Aggregates;

public class EventSourcingAggregateFeature : IAggregateFeature
{
	Aggregate? aggregate;
	// Versioning
	public int LastCommittedVersion { get; internal set; }
	public int CurrentVersion       { get; internal set; }
	// Snapshoting
	public bool  IsSnapshottable   => SnapshotType != null;
	public Type? SnapshotType      { get; internal set; }
	public int   SnapshotFrequency { get; internal set; }
	public int   SnapshotVersion   { get; internal set; }
	public void OnAttach(Aggregate aggregate)
	{
		this.aggregate = aggregate;
		if (!aggregate.HasEvents()) throw new AggregateFeatureNotFoundException($"{nameof(EventSourcingAggregateFeature)} requires {nameof(EventsAggregateFeature)}");
		aggregate.Events().Applying += (s, e) =>
		{
			if (!e.Value.HasEventSourcing()) e.Value.AddEventSourcing(CurrentVersion, Guid.NewGuid(), DateTime.UtcNow, 0);
		};
		aggregate.Events().Validated += (s, e) =>
		{
			if (CurrentVersion != e.Value.EventSourcing().TargetVersion)
				throw new AggregateStateMismatchException($"Aggregate '{aggregate.GetType().Name}' has version '{CurrentVersion}' and event has target version '{e.Value.EventSourcing().TargetVersion}'");
		};
		aggregate.Events().Pendent += (s, e) => CurrentVersion++;
	}
	// Hydrating
	internal void Hydrate(IEnumerable<Event> events)
	{
		foreach (var @event in events) aggregate?.ApplyEvent(@event.Replay());
		aggregate?.ClearPendingEvents();
		LastCommittedVersion = CurrentVersion;
	}
	public Snapshot GetSnapshot()
	{
		if (SnapshotType == null) throw new AggregateIsNotSnapshottableException();
		if (aggregate    == null) throw new InvalidStateException($"'{nameof(GetSnapshot)}' was called before this '{nameof(EventSourcingAggregateFeature)}' would be attached to an aggregate");
		var snap = (Snapshot?)Activator.CreateInstance(SnapshotType);
		if (snap == null) throw new InvalidProgramException("Instance of snapshot cannot be created");
		snap.AggregateId = aggregate.Id;
		snap.Version     = aggregate.GetCurrentVersion();
		snap.Load(aggregate);
		return snap;
	}
}
