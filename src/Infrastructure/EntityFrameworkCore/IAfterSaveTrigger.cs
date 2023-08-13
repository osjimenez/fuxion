using Microsoft.EntityFrameworkCore;

namespace Fuxion.EntityFrameworkCore;

public interface IAfterSaveTrigger<TContext> where TContext : DbContext
{
	Task Run(ITriggerState<TContext> state, CancellationToken cancellationToken = default);
}