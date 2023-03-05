using Fuxion.Application;
using Fuxion.Application.Commands;
using Fuxion.Domain;
using Orleans;
using Orleans.Concurrency;
using Orleans.EventSourcing;

namespace Fuxion.Orleans;

public class EntityGrain : JournaledGrain<EntityAggregate, Event>
{
	public EntityGrain()
	{
		
	}
	
}

public class EntityAggregate : Aggregate
{
	public List<Property> Properties                     { get; set; } = new();
	public void           AddProperty(Property property) => ApplyEvent(new PropertyAdded(Id));
}

public record PropertyAdded(Guid Id) : Event(Id);
public class Property
{
	
}
//
// [Reentrant]
// [StatelessWorker]
// public class OrleansDispatcher : Grain, ICommandDispatcher
// {
// 	readonly EntityFactory                factory;
// 	readonly IRepository<EntityAggregate> repository;
// 	public OrleansDispatcher(IRepository<EntityAggregate> repository, EntityFactory factory)
// 	{
// 		this.repository = repository;
// 		this.factory    = factory;
// 	}
// 	public async Task Handle(Command command)
// 	{
// 		var toDoTask = factory.Create(command.Id, command.ToDoTaskName);
// 		await repository.AddAsync(toDoTask);
// 		await repository.CommitAsync();
// 	}
// 	public Task DispatchAsync(Command command) => throw new NotImplementedException();
// }public class EntityFactory : Application.Factory<EntityAggregate>
// {
// 	public EntityFactory(IServiceProvider sp) : base(sp) { }
// 	public EntityFactory Create(Guid id, string name)
// 	{
// 		var toDoTask = Create(id);
// 		toDoTask.Create(name);
// 		return toDoTask;
// 	}
// }