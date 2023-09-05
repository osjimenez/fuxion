using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Fuxion.EntityFrameworkCore;

public static class ITriggerStateExtensions
{
	static List<(T Entity, PropertyValues OriginalValues)> ToListByState<T>(this ITriggerState me, EntityState? state) where T : class
	{
		if (me is IInternalTriggerState { Changes: not null } internalMe)
			return internalMe.Changes
				.Where(t => state == null || t.State == state)
				.Where(t => t.Entity is T)
				.Select(t => ((T)t.Entity, t.OriginalValues))
				.ToList();
		return me.DbContext.ChangeTracker.Entries<T>()
			.Where(e => state == null || e.State == state)
			.Select(e => (e.Entity, e.OriginalValues))
			.ToList();
	}
	public static List<T> Tracked<T>(this ITriggerState me) where T : class => me.ToListByState<T>(null).Select(_ => _.Entity).ToList();
	public static List<T> Added<T>(this ITriggerState me) where T : class => me.ToListByState<T>(EntityState.Added).Select(x => x.Entity).ToList();
	public static List<(T, PropertyValues)> Modified<T>(this ITriggerState me) where T : class => me.ToListByState<T>(EntityState.Modified);
	public static List<T> Unchanged<T>(this ITriggerState me) where T : class => me.ToListByState<T>(EntityState.Unchanged).Select(_ => _.Entity).ToList();
	public static List<T> Detached<T>(this ITriggerState me) where T : class => me.ToListByState<T>(EntityState.Detached).Select(_ => _.Entity).ToList();
	public static List<T> Deleted<T>(this ITriggerState me) where T : class => me.ToListByState<T>(EntityState.Deleted).Select(_ => _.Entity).ToList();
	public static async Task<List<TEntity>> LoadWithTracked<TEntity, TKey>(this ITriggerState me,
		List<TKey> keys,
		Func<TEntity, TKey> keySelector,
		Func<List<TKey>, List<Expression<Func<TEntity, bool>>>> expressionsBuilder,
		CancellationToken cancellationToken = default) where TEntity : class
	{
		var tracked = me.Tracked<TEntity>();
		var toLoad = keys.Except(tracked.Select(keySelector)).ToList();
		var query = me.DbContext.Set<TEntity>().AsQueryable();
		foreach (var expression in expressionsBuilder(toLoad)) query = query.Where(expression);
		return toLoad.Count > 0 ? tracked.Concat(await query.ToListAsync(cancellationToken)).ToList() : tracked;
	}
	public static async Task<List<TEntity>> LoadWithTracked<TEntity, TKey>(this ITriggerState me,
		List<TKey> keys,
		Func<TEntity, TKey> keySelector,
		Func<List<TKey>, Expression<Func<TEntity, bool>>> expressionBuilder,
		CancellationToken cancellationToken = default) where TEntity : class
	{
		var tracked = me.Tracked<TEntity>();
		var toLoad = keys.Except(tracked.Select(keySelector)).ToList();
		return toLoad.Count > 0 ? tracked.Concat(await me.DbContext.Set<TEntity>().Where(expressionBuilder(toLoad)).ToListAsync(cancellationToken)).ToList() : tracked;
	}
	public static async Task WhenAdded<T>(this ITriggerState me, Func<List<T>, Task> func) where T : class
	{
		var added = me.Added<T>();
		if (added.Count > 0) await func(added);
	}
	
	public static async Task WhenModified<T>(this ITriggerState me, Func<List<(T Entity, PropertyValues OriginalValues)>, Task> func) where T : class
	{
		var modified = me.Modified<T>();
		if(modified.Count > 0) await func(modified);
	}
	public static async Task WhenDeleted<T>(this ITriggerState me, Func<List<T>, Task> func) where T : class
	{
		var deleted = me.Deleted<T>();
		if (deleted.Count > 0) await func(deleted);
	}
}