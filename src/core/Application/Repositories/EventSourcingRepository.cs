using Fuxion.Application.Aggregates;
using Fuxion.Application.Events;
using Fuxion.Application.Factories;
using Fuxion.Application.Snapshots;
using Fuxion.Domain;
using Fuxion.Domain.Aggregates;

namespace Fuxion.Application.Repositories;

public class EventSourcingRepository<TAggregate> : IRepository<TAggregate> where TAggregate : IAggregate, new()
{
	public EventSourcingRepository(
		IEventStorage<TAggregate> eventStorage,
		ISnapshotStorage<TAggregate> snapshotStorage,
		IEventDispatcher eventDispatcher,
		Factory<TAggregate> aggregateFactory)
	{
		this.eventStorage = eventStorage;
		this.snapshotStorage = snapshotStorage;
		this.eventDispatcher = eventDispatcher;
		this.aggregateFactory = aggregateFactory;
	}
	readonly Factory<TAggregate> aggregateFactory;
	readonly IEventDispatcher eventDispatcher;
	readonly IEventStorage<TAggregate> eventStorage;
	readonly ISnapshotStorage<TAggregate> snapshotStorage;
	readonly Dictionary<Guid, IAggregate> trackedAggregates = new();
	public ILogger? Logger { get; set; }
	public void Dispose() { }
	public virtual async Task<TAggregate> GetAsync(Guid aggregateId)
	{
		TAggregate? aggregate = default;
		var startEvent = 0;
		if(aggregateFactory.Features().TryGet(out SnapshotFactoryFeature<TAggregate>? feature))
		{
			var snapshot = await snapshotStorage.GetSnapshotAsync(feature.Type, aggregateId);
			if (snapshot != null)
			{
				aggregate = feature.FromSnapshot((Snapshot<TAggregate>)snapshot);
				startEvent = snapshot.Version;
			}
		}
		var events = (await eventStorage.GetEventsAsync(aggregateId, startEvent, int.MaxValue)).ToList();
		if (aggregate == null && !events.Any()) throw new AggregateNotFoundException($"Aggregate with id '{aggregateId}' not found in repository");
		aggregate ??= aggregateFactory.Create(aggregateId);
		aggregate.Features().Get<EventSourcingAggregateFeature>().Hydrate(events);
		AddToTracking(aggregate);
		return aggregate;
	}
	public virtual Task AddAsync(TAggregate aggregate)
	{
		AddToTracking(aggregate);
		return Task.CompletedTask;
	}
	public async Task CommitAsync()
	{
		try
		{
			foreach (var aggregate in trackedAggregates.Values)
			{
				var feature = aggregate.Features().Get<EventSourcingAggregateFeature>();
				// if (!aggregate.HasEventSourcing())
				// 	throw new FeatureNotFoundException(
				// 		$"Aggregate '{aggregate.GetType().Name}:{aggregate.Id}' must has '{nameof(EventSourcingAggregateFeature)}' to uses '{nameof(EventSourcingRepository<TAggregate>)}'.");
				var expectedVersion = feature.LastCommittedVersion;
				// var expectedVersion = aggregate.GetLastCommittedVersion();
				var lastEvent = await eventStorage.GetLastEventAsync(aggregate.Id);
				if (lastEvent is not null)
				{
					var evtFeature = lastEvent.Features().Get<EventSourcingEventFeature>();
					if(expectedVersion == 0)
						throw new AggregateCreationException($"Aggregate '{aggregate.Id}' can't be created as it already exists with version {evtFeature.TargetVersion + 1}");
					if (evtFeature.TargetVersion + 1 != expectedVersion)
						throw new ConcurrencyException($"Aggregate '{aggregate.Id}' has been modified externally and has an updated state. Can't commit changes.");
				}
				// if (lastEvent != null && expectedVersion == 0)
				// 	throw new AggregateCreationException($"Aggregate '{aggregate.Id}' can't be created as it already exists with version {lastEvent.EventSourcing().TargetVersion + 1}");
				// if (lastEvent != null && lastEvent.EventSourcing().TargetVersion + 1 != expectedVersion)
				// 	throw new ConcurrencyException($"Aggregate '{aggregate.Id}' has been modified externally and has an updated state. Can't commit changes.");
				var changesToCommit = aggregate.Features().Get<EventsAggregateFeature>().GetPendingEvents();
				//var changesToCommit = aggregate.GetPendingEvents();

				//perform pre commit actions
				foreach (var e in changesToCommit) e.Features().Get<EventSourcingEventFeature>().EventCommittedTimestamp = DateTime.UtcNow;

				//CommitAsync events to storage provider
				await eventStorage.CommitAsync(aggregate.Id, changesToCommit);
				feature.LastCommittedVersion = feature.CurrentVersion;

				// If the Aggregate is snapshottable
				if (feature.IsSnapshottable)
					if (feature.CurrentVersion - feature.SnapshotVersion > feature.SnapshotFrequency)
					{
						await snapshotStorage.SaveSnapshotAsync(feature.GetSnapshot());
						feature.SnapshotVersion = feature.CurrentVersion;
					}
				// if (aggregate.IsSnapshottable())
				// 	if (aggregate.GetCurrentVersion() - aggregate.GetSnapshotVersion() > aggregate.GetSnapshotFrequency())
				// 	{
				// 		await snapshotStorage.SaveSnapshotAsync(aggregate.GetSnapshot());
				// 		aggregate.EventSourcing().SnapshotVersion = aggregate.GetCurrentVersion();
				// 	}

				// Dispatch events asynchronously
				foreach (var e in changesToCommit) await eventDispatcher.DispatchAsync(e);
				aggregate.Features().Get<EventsAggregateFeature>().ClearPendingEvents();
				// aggregate.ClearPendingEvents();
			}
			trackedAggregates.Clear();
		} catch (Exception ex)
		{
			Logger?.LogError(ex, $"Error '{ex.GetType().Name}' in '{nameof(EventSourcingRepository<TAggregate>)}.{nameof(CommitAsync)}': {ex.Message}");
			throw;
		}
	}
	bool IsTracked(Guid id) => trackedAggregates.ContainsKey(id);
	public void AddToTracking(TAggregate aggregate)
	{
		if (!IsTracked(aggregate.Id))
			trackedAggregates.Add(aggregate.Id, aggregate);
		else throw new ConcurrencyException("Aggregate can't be added because it's already tracked.");
		//else if (trackedAggregates[aggregate.Id] != aggregate) throw new ConcurrencyException("Aggregate can't be added because it's already tracked.");
	}
}