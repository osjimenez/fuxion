using Fuxion.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Application
{
	public interface IRepository<TAggregate> : IDisposable where TAggregate : Aggregate
	{
		Task<TAggregate> GetAsync(Guid aggregateId);
		Task AddAsync(TAggregate aggregate);
		Task CommitAsync();
	}
}