namespace Fuxion.Application;

using Fuxion.Domain;

public interface IRepository<TAggregate> : IDisposable where TAggregate : Aggregate
{
	Task<TAggregate> GetAsync(Guid aggregateId);
	Task AddAsync(TAggregate aggregate);
	Task CommitAsync();
}