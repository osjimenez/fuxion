using Fuxion.Domain;

namespace Fuxion.Application.Events;

class EventStorageDecorator<TAggregate> : IEventStorage<TAggregate> where TAggregate : Aggregate
{
	public EventStorageDecorator(IEventStorage storage) => this.storage = storage;
	readonly IEventStorage           storage;
	public   Task<IQueryable<Event>> GetEventsAsync(Guid    aggregateId, int start, int count) => storage.GetEventsAsync(aggregateId, start, count);
	public   Task<Event?>            GetLastEventAsync(Guid aggregateId)                            => storage.GetLastEventAsync(aggregateId);
	public   Task                    CommitAsync(Guid       aggregateId, IEnumerable<Event> events) => storage.CommitAsync(aggregateId, events);
}