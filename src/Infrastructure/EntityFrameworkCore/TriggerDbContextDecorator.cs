using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Fuxion.EntityFrameworkCore;

public class TriggerDbContextDecorator<TDbContext> : IDisposable, IAsyncDisposable where TDbContext : DbContext
{
	readonly IEnumerable<IAfterSaveTrigger<TDbContext>> _afterSaveTriggers;
	readonly IEnumerable<IBeforeSaveTrigger<TDbContext>> _beforeSaveTriggers;
	public TriggerDbContextDecorator(TDbContext context, IEnumerable<IBeforeSaveTrigger<TDbContext>> beforeSaveTriggers, IEnumerable<IAfterSaveTrigger<TDbContext>> afterSaveTriggers)
	{
		Context = context;
		_beforeSaveTriggers = beforeSaveTriggers;
		_afterSaveTriggers = afterSaveTriggers;
	}
	public TDbContext Context { get; }
	public DatabaseFacade Database => Context.Database;
	public IModel Model => Context.Model;
	public ChangeTracker ChangeTracker => Context.ChangeTracker;
	public DbContextId ContextId => Context.ContextId;
	public ValueTask DisposeAsync() => Context.DisposeAsync();
	public void Dispose() => Context.Dispose();
	public int SaveChangesTriggered()
	{
		foreach (var trigger in _beforeSaveTriggers) trigger.Run(Context).Wait();
		var changes = Context.ChangeTracker.Entries().Select(e => (e.Entity, e.State)).ToList();
		var res = Context.SaveChanges();
		foreach (var trigger in _afterSaveTriggers) trigger.Run(Context, changes).Wait();
		return res;
	}
	public async Task<int> SaveChangesTriggeredAsync(CancellationToken cancellationToken = default)
	{
		foreach (var trigger in _beforeSaveTriggers) await trigger.Run(Context, cancellationToken);
		var changes = Context.ChangeTracker.Entries().Select(e => (e.Entity, e.State)).ToList();
		var res = await Context.SaveChangesAsync(cancellationToken);
		foreach (var trigger in _afterSaveTriggers) await trigger.Run(Context, changes, cancellationToken);
		return res;
	}
	public DbSet<TEntity> Set<TEntity>() where TEntity : class => Context.Set<TEntity>();
	public EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class => Context.Add(entity);
	public ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity) where TEntity : class => Context.AddAsync(entity);
	public void AddRange(params object[] entities) => Context.AddRange(entities);
	public Task AddRangeAsync(params object[] entities) => Context.AddRangeAsync(entities);
	public EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class => Context.Attach(entity);
	public void AttachRange(params object[] entities) => Context.AttachRange(entities);
	public TEntity? Find<TEntity>(params object?[]? keyValues) where TEntity : class => Context.Find<TEntity>(keyValues);
	public object? Find(Type entityType, params object?[]? keyValues) => Context.Find(entityType, keyValues);
	public ValueTask<TEntity?> FindAsync<TEntity>(Type entityType, params object?[]? keyValues) where TEntity : class => Context.FindAsync<TEntity>(entityType, keyValues);
	public ValueTask<object?> FindAsync(Type entityType, params object?[]? keyValues) => Context.FindAsync(entityType, keyValues);
	public EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class => Context.Update(entity);
	public void UpdateRange(params object[] entities) => Context.UpdateRange(entities);
	public EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class => Context.Remove(entity);
	public void RemoveRange(params object[] entities) => Context.RemoveRange(entities);
	public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class => Context.Entry(entity);
	public IQueryable<TResult> RemoveRange<TResult>(Expression<Func<IQueryable<TResult>>> expression) => Context.FromExpression(expression);
}