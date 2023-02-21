using Microsoft.EntityFrameworkCore;

namespace Fuxion.EntityFrameworkCore;

public interface IBeforeSaveTrigger<in TContext> where TContext : DbContext
{
	Task Run(TContext db, CancellationToken cancellationToken = default);
}