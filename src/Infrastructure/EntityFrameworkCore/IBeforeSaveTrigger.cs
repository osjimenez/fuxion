using Microsoft.EntityFrameworkCore;

namespace Fuxion.EntityFrameworkCore;

public interface IBeforeSaveTrigger<TContext> where TContext : DbContext
{
	Task Run(ITriggerState<TContext> state, CancellationToken cancellationToken = default);
}