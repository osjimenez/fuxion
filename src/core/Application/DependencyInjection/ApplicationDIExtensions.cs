using Fuxion.Application;
using Fuxion.Application.Commands;
using Fuxion.Application.Events;
using Fuxion.Application.Factories;
using Fuxion.Application.Repositories;
using Fuxion.Application.Snapshots;
using Fuxion.Domain;
using Fuxion.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationDIExtensions
{
	public static IServiceCollection AddFuxion(this IServiceCollection me, Action<IFuxionBuilder> builderAction)
	{
		var typeKeyDirectory = new TypeKeyDirectory();
		me.AddSingleton(typeKeyDirectory);
		var builder = new FuxionBuilder(me, typeKeyDirectory);
		builderAction(builder);
		foreach (var action in builder.PreRegistrationsList)
		{
			var sp = me.BuildServiceProvider();
			action(sp);
		}
		foreach (var action in builder.RegistrationsList)
		{
			var sp = me.BuildServiceProvider();
			action(sp);
		}
		foreach (var tup in builder.AutoActivateList)
		{
			var sp = me.BuildServiceProvider();
			tup.PreAction?.Invoke(sp);
			var obj = sp.GetRequiredService(tup.Type);
			tup.PostAction?.Invoke(sp, obj);
		}
		return me;
	}
	public static IFuxionBuilder InMemoryEventStorage(this IFuxionBuilder me, out Func<IServiceProvider, InMemoryEventStorage> builder, string? dumpFilePath = null)
	{
		builder = sp => new(sp.GetRequiredService<TypeKeyDirectory>(), dumpFilePath);
		me.Services.AddSingleton(builder);
		return me;
	}
	public static IFuxionBuilder InMemorySnapshotStorage(this IFuxionBuilder me, out Func<IServiceProvider, InMemorySnapshotStorage> builder, string? dumpFilePath = null)
	{
		builder = sp => new(sp.GetRequiredService<TypeKeyDirectory>(), dumpFilePath);
		me.Services.AddSingleton(builder);
		return me;
	}
	public static IFuxionBuilder Aggregate<TAggregate, TAggregateFactory>(this IFuxionBuilder me, Func<IServiceProvider, IEventPublisher>? eventPublisher = null)
		where TAggregate : Aggregate, new() where TAggregateFactory : Factory<TAggregate>
	{
		me.Services.AddScoped<TAggregateFactory>();
		me.Services.AddScoped<Factory<TAggregate>>(sp => sp.GetRequiredService<TAggregateFactory>());
		if (eventPublisher != null)
		{
			me.Services.AddTransient<IFactoryFeature<TAggregate>, EventsFactoryFeature<TAggregate>>();
			//AggregateFactory<TAggregate>.Initializer.OnInitialize(a => a.AttachEvents());
			// I don't have to register IEventPublisher, i must register IEventPublisher<TAggregate>. Because of this, i use the decorator.
			me.Services.AddSingleton<IEventPublisher<TAggregate>>(sp => new EventPublisherDecorator<TAggregate>(eventPublisher!(sp)));
		}
		return me;
	}
	public static IFuxionBuilder Aggregate<TAggregate, TAggregateFactory>(this IFuxionBuilder                      me, Func<IServiceProvider, IEventStorage> eventStorage,
																								 Func<IServiceProvider, IEventPublisher>? eventPublisher = null) where TAggregate : Aggregate, new()
		where TAggregateFactory : Factory<TAggregate>
	{
		Aggregate<TAggregate, TAggregateFactory>(me, eventPublisher);
		me.Services.AddScoped<IRepository<TAggregate>, EventSourcingRepository<TAggregate>>();
		me.Services.AddTransient<IFactoryFeature<TAggregate>, EventSourcingFactoryFeature<TAggregate>>();
		// I don't have to register IEventStorage, i must register IEventStorage<TAggregate>. Because of this, i use the decorator.
		me.Services.AddSingleton<IEventStorage<TAggregate>>(sp => new EventStorageDecorator<TAggregate>(eventStorage(sp)));
		return me;
	}
	public static IFuxionBuilder Aggregate<TAggregate, TAggregateFactory, TSnapshot>(this IFuxionBuilder                      me,              Func<IServiceProvider, IEventStorage> eventStorage,
																												Func<IServiceProvider, ISnapshotStorage> snapshotStorage, int snapshotFrecuency = 3,
																												Func<IServiceProvider, IEventPublisher>? eventPublisher = null) where TAggregate : Aggregate, new()
		where TAggregateFactory : Factory<TAggregate>
		where TSnapshot : Snapshot<TAggregate>
	{
		Aggregate<TAggregate, TAggregateFactory>(me, eventStorage, eventPublisher);
		me.TypeKeyDirectory.Register<TSnapshot>();
		me.Services.AddSingleton<IFactoryFeature<TAggregate>>(sp => new SnapshotFactoryFeature<TAggregate>(typeof(TSnapshot), snapshotFrecuency));
		// I don't have to register ISnapshotStorage, i must register ISnapshotStorage<TAggregate>. Because of this, i use the decorator.
		me.Services.AddSingleton<ISnapshotStorage<TAggregate>>(sp => new SnapshotStorageDecorator<TAggregate>(snapshotStorage(sp)));
		return me;
	}
	public static IFuxionBuilder Aggregate<TAggregate, TAggregateFactory, TAggregateRepository>(this IFuxionBuilder me, Func<IServiceProvider, IEventPublisher>? eventPublisher = null)
		where TAggregate : Aggregate, new() where TAggregateFactory : Factory<TAggregate> where TAggregateRepository : class, IRepository<TAggregate>
	{
		Aggregate<TAggregate, TAggregateFactory>(me, eventPublisher);
		me.Services.AddScoped<TAggregateRepository>();
		me.Services.AddScoped<IRepository<TAggregate>>(sp => sp.GetRequiredService<TAggregateRepository>());
		return me;
	}
	public static IFuxionBuilder Events(this IFuxionBuilder me, Action<IEventsBuilder> builderAction)
	{
		me.Services.AddScoped<IEventDispatcher, EventDispatcher>();
		builderAction(new EventsBuilder(me));
		return me;
	}
	public static IEventsBuilder HandlersFromType(this IEventsBuilder me, Type typeOfEventHandlers)
	{
		foreach (var inter in typeOfEventHandlers.GetInterfaces().Where(t => t.IsSubclassOfRawGeneric(typeof(IEventHandler<>))))
		{
			me.FuxionBuilder.Services.AddScoped(inter, typeOfEventHandlers);
			if (me.FuxionBuilder.TypeKeyDirectory.ContainsKey(inter.GetGenericArguments()[0].GetTypeKey()))
			{
				if (me.FuxionBuilder.TypeKeyDirectory[inter.GetGenericArguments()[0].GetTypeKey()] != inter.GetGenericArguments()[0]) throw new InvalidProgramException("");
			} else
				me.FuxionBuilder.TypeKeyDirectory.Register(inter.GetGenericArguments()[0]);
		}
		return me;
	}
	public static IEventsBuilder HandlersFromType<T>(this IEventsBuilder me) => HandlersFromType(me, typeof(T));
	public static IEventsBuilder HandlersFromAssemblyOf(this IEventsBuilder me, Type typeOfAssembly)
	{
		foreach (var handler in typeOfAssembly.Assembly.GetTypes().Where(t => t.IsSubclassOfRawGeneric(typeof(IEventHandler<>)))) HandlersFromType(me, handler);
		return me;
	}
	public static IEventsBuilder HandlersFromAssemblyOf<T>(this IEventsBuilder me) => HandlersFromAssemblyOf(me, typeof(T));
	public static IEventsBuilder Subscribe<TEvent>(this IEventsBuilder me, Func<IServiceProvider, IEventSubscriber> eventSubscriber) where TEvent : Event
	{
		me.FuxionBuilder.Services.AddTransient(sp => new EventSubscription(typeof(TEvent)));
		return me;
	}
	public static IFuxionBuilder Commands(this IFuxionBuilder me, Action<ICommandsBuilder> builderAction)
	{
		me.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();
		builderAction(new CommandsBuilder(me));
		return me;
	}
	public static ICommandsBuilder HandlersFromAssemblyOf<T>(this ICommandsBuilder me)
	{
		foreach (var handler in typeof(T).Assembly.GetTypes().Where(t => t.IsSubclassOfRawGeneric(typeof(ICommandHandler<>))))
		foreach (var inter in handler.GetInterfaces().Where(t => t.IsSubclassOfRawGeneric(typeof(ICommandHandler<>))))
		{
			me.FuxionBuilder.Services.AddScoped(typeof(ICommandHandler<>).MakeGenericType(inter.GetGenericArguments()[0]), handler);
			me.FuxionBuilder.TypeKeyDirectory.Register(inter.GetGenericArguments()[0]);
		}
		return me;
	}
}

public interface IFuxionBuilder
{
	IServiceCollection Services         { get; }
	TypeKeyDirectory   TypeKeyDirectory { get; }
	void               AddToPreRegistrationList(Action<IServiceProvider>  action);
	void               AddToRegistrationList(Action<IServiceProvider>     action);
	void               AddToAutoActivateList<T>(Action<IServiceProvider>? preAction = null, Action<IServiceProvider, T>? postAction = null);
}

class FuxionBuilder : IFuxionBuilder
{
	public FuxionBuilder(IServiceCollection services, TypeKeyDirectory typeKeyDirectory)
	{
		Services         = services;
		TypeKeyDirectory = typeKeyDirectory;
	}
	public List<Action<IServiceProvider>> PreRegistrationsList = new();
	public List<Action<IServiceProvider>> RegistrationsList = new();
	public List<(Type Type, Action<IServiceProvider>? PreAction, Action<IServiceProvider, object>? PostAction)> AutoActivateList { get; } = new();
	public IServiceCollection Services { get; }
	public TypeKeyDirectory TypeKeyDirectory { get; }
	public void AddToPreRegistrationList(Action<IServiceProvider> action) => PreRegistrationsList.Add(action);
	public void AddToRegistrationList(Action<IServiceProvider> action) => RegistrationsList.Add(action);
	public void AddToAutoActivateList<T>(Action<IServiceProvider>? preAction = null, Action<IServiceProvider, T>? postAction = null) =>
		AutoActivateList.Add((typeof(T), preAction, postAction != null ? new Action<IServiceProvider, object>((sp, o) => postAction(sp, (T)o)) : null));
}

public interface IEventsBuilder
{
	IFuxionBuilder FuxionBuilder { get; }
}

class EventsBuilder : IEventsBuilder
{
	public EventsBuilder(IFuxionBuilder fuxionBuilder) => FuxionBuilder = fuxionBuilder;
	public IFuxionBuilder FuxionBuilder { get; }
}

public interface ICommandsBuilder
{
	IFuxionBuilder FuxionBuilder { get; }
}

class CommandsBuilder : ICommandsBuilder
{
	public CommandsBuilder(IFuxionBuilder fuxionBuilder) => FuxionBuilder = fuxionBuilder;
	public IFuxionBuilder FuxionBuilder { get; }
}