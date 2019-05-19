using Fuxion.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fuxion.Application.Events
{
	internal class EventStorageDecorator<TAggregate> : IEventStorage<TAggregate> where TAggregate : Aggregate
	{
		public EventStorageDecorator(IEventStorage storage)
		{
			this.storage = storage;
		}
		IEventStorage storage;
		public Task<IQueryable<Event>> GetEventsAsync(Guid aggregateId, int start, int count) => storage.GetEventsAsync(aggregateId, start, count);
		public Task<Event?> GetLastEventAsync(Guid aggregateId) => storage.GetLastEventAsync(aggregateId);
		public Task CommitAsync(Guid aggregateId, IEnumerable<Event> events) => storage.CommitAsync(aggregateId, events);
	}
}
