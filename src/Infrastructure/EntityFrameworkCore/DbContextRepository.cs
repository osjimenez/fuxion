using Fuxion.Application;
using Fuxion.Domain;
using Fuxion.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Fuxion.EntityFrameworkCore;

public abstract class DbContextRepository<TContext, TAggregate> : IRepository<TAggregate> where TContext : DbContext where TAggregate : Aggregate, new()
{
	public DbContextRepository(TContext context, IEventDispatcher eventDispatcher)
	{
		Context         = context;
		EventDispatcher = eventDispatcher;
	}
	readonly  List<TAggregate> attached = new();
	protected TContext         Context         { get; }
	protected IEventDispatcher EventDispatcher { get; }
	async Task<TAggregate> IRepository<TAggregate>.GetAsync(Guid aggregateId)
	{
		var aggregate = await GetAsync(aggregateId);
		attached.Add(aggregate);
		return aggregate;
	}
	async Task IRepository<TAggregate>.AddAsync(TAggregate aggregate)
	{
		await AddAsync(aggregate);
		attached.Add(aggregate);
	}
	async Task IRepository<TAggregate>.CommitAsync()
	{
		var changesToCommit = attached.SelectMany(a => a.GetPendingEvents());
		await Context.SaveChangesAsync();

		// Dispatch events asynchronously
		foreach (var e in changesToCommit) await EventDispatcher.DispatchAsync(e);
	}
	public          void             Dispose() => Context.Dispose();
	public abstract Task<TAggregate> GetAsync(Guid       aggregateId);
	public abstract Task             AddAsync(TAggregate aggregate);
}