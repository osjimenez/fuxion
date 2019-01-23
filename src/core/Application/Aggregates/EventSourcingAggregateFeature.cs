using Fuxion.Application.Events;
using Fuxion.Application.Snapshots;
using Fuxion.Domain;
using Fuxion.Domain.Aggregates;
using Fuxion.Domain.Events;
using System;
using System.Collections.Generic;

namespace Fuxion.Application.Aggregates
{
	public class EventSourcingAggregateFeature : IAggregateFeature
	{
		private Aggregate aggregate;

		public void OnAttach(Aggregate aggregate)
		{
			this.aggregate = aggregate;

			if (!aggregate.HasEvents())
			{
				throw new AggregateFeatureNotFoundException($"{nameof(EventSourcingAggregateFeature)} requires {nameof(EventsAggregateFeature)}");
			}
			aggregate.Events().Applying += (s, e) =>
			{
				if (!e.Value.HasEventSourcing())
					e.Value.AddEventSourcing(CurrentVersion, Guid.NewGuid(), DateTime.UtcNow, 0);
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
			foreach (var @event in events)
				aggregate.ApplyEvent(@event.Replay());
			aggregate.ClearPendingEvents();
			LastCommittedVersion = CurrentVersion;
		}
		// Versioning
		public int LastCommittedVersion { get; internal set; }
		public int CurrentVersion { get; internal set; }
		// Snapshoting
		public bool IsSnapshottable => SnapshotType != null;
		public Type SnapshotType { get; internal set; }
		public int SnapshotFrequency { get; internal set; }
		public int SnapshotVersion { get; internal set; }
		public Snapshot GetSnapshot()
		{
			var snap = (Snapshot)Activator.CreateInstance(SnapshotType);
			snap.AggregateId = aggregate.Id;
			snap.Version = aggregate.GetCurrentVersion();
			snap.Load(aggregate);
			return snap;
		}
	}
}