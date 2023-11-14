using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Fuxion.EntityFrameworkCore;

public interface ITriggerState
{
	DbContext DbContext { get; }
}

public interface IInternalTriggerState : ITriggerState
{
	List<(object Entity, EntityState State, PropertyValues OriginalValues)>? Changes { get; }
}
public interface ITriggerState<out TDbContext> : ITriggerState where TDbContext : DbContext
{
	DbContext ITriggerState.DbContext => DbContext;
	new TDbContext DbContext { get; }
}
class TriggerState<TDbContext>(TDbContext context) : IInternalTriggerState, ITriggerState<TDbContext>
	where TDbContext : DbContext
{
	DbContext ITriggerState.DbContext => DbContext;
	public TDbContext DbContext { get; } = context;
	public List<(object Entity, EntityState State, PropertyValues OriginalValues)>? Changes { get; private set; } = null;
	public int SaveChanges()
	{
		Changes = DbContext.ChangeTracker.Entries().Select(e => (e.Entity, e.State, e.OriginalValues)).ToList();
		return DbContext.SaveChanges();
	}
	public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
	{
		Changes = DbContext.ChangeTracker.Entries().Select(e => (e.Entity, e.State, e.OriginalValues)).ToList();
		return DbContext.SaveChangesAsync(cancellationToken);
	}
}