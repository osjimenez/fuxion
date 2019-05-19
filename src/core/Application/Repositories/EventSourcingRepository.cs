using Fuxion.Application.Aggregates;
using Fuxion.Application.Events;
using Fuxion.Application.Factories;
using Fuxion.Application.Snapshots;
using Fuxion.Domain;
using Fuxion.Domain.Aggregates;
using Fuxion.Domain.Events;
using Fuxion.Reflection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fuxion.Application.Repositories
{
	public class EventSourcingRepository<TAggregate> : IRepository<TAggregate> where TAggregate : Aggregate, new()
	{
		public EventSourcingRepository(IEventStorage<TAggregate> eventStorage, ISnapshotStorage<TAggregate> snapshotStorage, IEventDispatcher eventDispatcher, TypeKeyDirectory typeKeyDirectory, Factory<TAggregate> aggregateFactory)
		{
			this.eventStorage = eventStorage;
			this.snapshotStorage = snapshotStorage;
			this.eventDispatcher = eventDispatcher;
			this.typeKeyDirectory = typeKeyDirectory;
			this.aggregateFactory = aggregateFactory;
		}
		public void Dispose() { }
		private readonly IEventDispatcher eventDispatcher;
		private readonly IEventStorage<TAggregate> eventStorage;
		private readonly ISnapshotStorage<TAggregate> snapshotStorage;
		private readonly TypeKeyDirectory typeKeyDirectory;
		private readonly Factory<TAggregate> aggregateFactory;
		private readonly Dictionary<Guid, Aggregate> trackedAggregates = new Dictionary<Guid, Aggregate>();
		public ILogger? Logger { get; set; }
		public virtual async Task<TAggregate> GetAsync(Guid aggregateId)
		{
			TAggregate? aggregate = null;
			var startEvent = 0;
			if (aggregateFactory.IsSnapshottable())
			{
				var snapshot = await snapshotStorage.GetSnapshotAsync(aggregateFactory.GetSnapshotType(), aggregateId);
				if (snapshot != null)
				{
					aggregate = aggregateFactory.FromSnapshot((Snapshot<TAggregate>)snapshot);
					startEvent = snapshot.Version;
				}
			}
			var events = (await eventStorage.GetEventsAsync(aggregateId, startEvent, int.MaxValue)).ToList();
			if (aggregate == null && !events.Any()) throw new AggregateNotFoundException($"Aggregate with id '{aggregateId}' not found in repository");
			aggregate ??= aggregateFactory.Create(aggregateId);
			aggregate.Hydrate(events);
			AddToTracking(aggregate);
			return aggregate;
		}
		public virtual Task AddAsync(TAggregate aggregate)
		{
			AddToTracking(aggregate);
			return Task.CompletedTask;
		}
		private bool IsTracked(Guid id) => trackedAggregates.ContainsKey(id);
		public void AddToTracking(TAggregate aggregate)
		{
			if (!IsTracked(aggregate.Id))
				trackedAggregates.Add(aggregate.Id, aggregate);
			else if (trackedAggregates[aggregate.Id] != aggregate)
				throw new ConcurrencyException($"Aggregate can't be added because it's already tracked.");
		}
		public async Task CommitAsync()
		{
			try
			{
				foreach (var aggregate in trackedAggregates.Values)
				{
					if (!aggregate.HasEventSourcing())
						throw new AggregateFeatureNotFoundException($"Aggregate '{aggregate.GetType().Name}:{aggregate.Id}' must has '{nameof(EventSourcingAggregateFeature)}' to uses '{nameof(EventSourcingRepository<TAggregate>)}'.");
					var expectedVersion = aggregate.GetLastCommittedVersion();
					var lastEvent = await eventStorage.GetLastEventAsync(aggregate.Id);
					if ((lastEvent != null) && (expectedVersion == 0))
					{
						throw new AggregateCreationException($"Aggregate '{aggregate.Id}' can't be created as it already exists with version {lastEvent.EventSourcing().TargetVersion + 1}");
					}
					else if ((lastEvent != null) && ((lastEvent.EventSourcing().TargetVersion + 1) != expectedVersion))
					{
						throw new ConcurrencyException($"Aggregate '{aggregate.Id}' has been modified externally and has an updated state. Can't commit changes.");
					}
					var changesToCommit = aggregate.GetPendingEvents();

					//perform pre commit actions
					foreach (var e in changesToCommit)
					{
						e.EventSourcing().EventCommittedTimestamp = DateTime.UtcNow;
					}

					//CommitAsync events to storage provider
					await eventStorage.CommitAsync(aggregate.Id, changesToCommit);

					aggregate.EventSourcing().LastCommittedVersion = aggregate.GetCurrentVersion();

					// If the Aggregate is snapshottable
					if (aggregate.IsSnapshottable())
					{
						if (aggregate.GetCurrentVersion() - aggregate.GetSnapshotVersion() > aggregate.GetSnapshotFrequency())
						{
							await snapshotStorage.SaveSnapshotAsync(aggregate.GetSnapshot());
							aggregate.EventSourcing().SnapshotVersion = aggregate.GetCurrentVersion();
						}
					}

					// Dispatch events asynchronously
					foreach (var e in changesToCommit)
					{
						await eventDispatcher.DispatchAsync(e);
					}

					aggregate.ClearPendingEvents();
				}
				trackedAggregates.Clear();
			}
			catch (Exception ex)
			{
				Logger?.LogError(ex, $"Error '{ex.GetType().Name}' in '{nameof(EventSourcingRepository<TAggregate>)}.{nameof(CommitAsync)}': {ex.Message}");
				throw;
			}
		}
	}
}
