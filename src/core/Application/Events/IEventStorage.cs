namespace Fuxion.Application.Events;

using Fuxion.Domain;

public interface IEventStorage
{
	Task<IQueryable<Event>> GetEventsAsync(Guid aggregateId, int start, int count);
	Task<Event?> GetLastEventAsync(Guid aggregateId);
	Task CommitAsync(Guid aggregateId, IEnumerable<Event> events);
}
public interface IEventStorage<TAggregate> : IEventStorage where TAggregate : Aggregate { }
