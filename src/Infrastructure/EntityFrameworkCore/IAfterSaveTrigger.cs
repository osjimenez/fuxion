using Microsoft.EntityFrameworkCore;

namespace Fuxion.EntityFrameworkCore;

public interface IAfterSaveTrigger<in TContext> where TContext : DbContext
{
	Task Run(TContext db, List<(object Entity, EntityState State)> changes, CancellationToken cancellationToken = default);
}