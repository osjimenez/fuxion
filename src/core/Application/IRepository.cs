using Fuxion.Domain;

namespace Fuxion.Application;

public interface IRepository<TAggregate> : IDisposable where TAggregate : Aggregate
{
	Task<TAggregate> GetAsync(Guid aggregateId);
	Task AddAsync(TAggregate aggregate);
	Task CommitAsync();
}