using Fuxion.Reflection;
using Fuxion.Application;
using Fuxion.Application.Commands;
using Fuxion.Application.Events;
using Fuxion.Application.Factories;
using Fuxion.Application.Repositories;
using Fuxion.Application.Snapshots;
using Fuxion.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class ApplicationDIExtensions
	{
		public static IServiceCollection AddFuxion(this IServiceCollection me, Action<IFuxionBuilder> builderAction)
		{
			var typeKeyDirectory = new TypeKeyDirectory();
			me.AddSingleton(typeKeyDirectory);
			var builder = new FuxionBuilder(me, typeKeyDirectory);
			builderAction(builder);
			foreach (var type in builder.AutoActivateList)
				me.BuildServiceProvider().GetRequiredService(type);
			return me;
		}

		public static IFuxionBuilder InMemoryEventStorage(this IFuxionBuilder me, out Func<IServiceProvider, InMemoryEventStorage> builder, string dumpFilePath = null)
		{
			builder = new Func<IServiceProvider, InMemoryEventStorage>(sp => new InMemoryEventStorage(sp.GetRequiredService<TypeKeyDirectory>(), dumpFilePath));
			me.Services.AddSingleton(builder);
			return me;
		}
		public static IFuxionBuilder InMemorySnapshotStorage(this IFuxionBuilder me, out Func<IServiceProvider, InMemorySnapshotStorage> builder, string dumpFilePath = null)
		{
			builder = new Func<IServiceProvider, InMemorySnapshotStorage>(sp => new InMemorySnapshotStorage(sp.GetRequiredService<TypeKeyDirectory>(), dumpFilePath));
			me.Services.AddSingleton(builder);
			return me;
		}

		public static IFuxionBuilder Aggregate<TAggregate, TAggregateFactory>(
			this IFuxionBuilder me,
			Func<IServiceProvider, IEventPublisher> eventPublisher = null)
			where TAggregate : Aggregate, new()
			where TAggregateFactory : Factory<TAggregate>
		{
			me.Services.AddScoped<TAggregateFactory>();
			me.Services.AddScoped<Factory<TAggregate>>(sp => sp.GetRequiredService<TAggregateFactory>());

			if (eventPublisher != null)
			{
				me.Services.AddTransient<IFactoryFeature<TAggregate>, EventsFactoryFeature<TAggregate>>();
				//AggregateFactory<TAggregate>.Initializer.OnInitialize(a => a.AttachEvents());
				// I don't have to register IEventPublisher, i must register IEventPublisher<TAggregate>. Because of this, i use the decorator.
				me.Services.AddSingleton<IEventPublisher<TAggregate>>(sp => new EventPublisherDecorator<TAggregate>(eventPublisher(sp)));
			}
			return me;
		}
		public static IFuxionBuilder Aggregate<TAggregate, TAggregateFactory>(
			this IFuxionBuilder me,
			Func<IServiceProvider, IEventStorage> eventStorage,
			Func<IServiceProvider, IEventPublisher> eventPublisher = null)
			where TAggregate : Aggregate, new()
			where TAggregateFactory : Factory<TAggregate>
		{
			Aggregate<TAggregate, TAggregateFactory>(me, eventPublisher);
			me.Services.AddScoped<IRepository<TAggregate>, EventSourcingRepository<TAggregate>>();
			me.Services.AddTransient<IFactoryFeature<TAggregate>, EventSourcingFactoryFeature<TAggregate>>();
			// I don't have to register IEventStorage, i must register IEventStorage<TAggregate>. Because of this, i use the decorator.
			me.Services.AddSingleton<IEventStorage<TAggregate>>(sp => new EventStorageDecorator<TAggregate>(eventStorage(sp)));
			return me;
		}
		public static IFuxionBuilder Aggregate<TAggregate, TAggregateFactory, TSnapshot>(
			this IFuxionBuilder me,
			Func<IServiceProvider, IEventStorage> eventStorage,
			Func<IServiceProvider, ISnapshotStorage> snapshotStorage,
			int snapshotFrecuency = 3,
			Func<IServiceProvider, IEventPublisher> eventPublisher = null)
			where TAggregate : Aggregate, new()
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
		public static IFuxionBuilder Aggregate<TAggregate, TAggregateFactory, TAggregateRepository>(
			this IFuxionBuilder me,
			Func<IServiceProvider, IEventPublisher> eventPublisher = null)
			where TAggregate : Aggregate, new()
			where TAggregateFactory : Factory<TAggregate>
			where TAggregateRepository : class, IRepository<TAggregate>
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
		public static IEventsBuilder HandlersFromAssemblyOf<T>(this IEventsBuilder me)
		{
			foreach (var handler in typeof(T).Assembly.GetTypes().Where(t => t.IsSubclassOfRawGeneric(typeof(IEventHandler<>))))
			{
				foreach (var inter in handler.GetInterfaces().Where(t => t.IsSubclassOfRawGeneric(typeof(IEventHandler<>))))
				{
					me.FuxionBuilder.Services.AddScoped(inter, handler);
					if (me.FuxionBuilder.TypeKeyDirectory.ContainsKey(inter.GetGenericArguments()[0].GetTypeKey()))
					{
						if (me.FuxionBuilder.TypeKeyDirectory[inter.GetGenericArguments()[0].GetTypeKey()] != inter.GetGenericArguments()[0])
							throw new InvalidProgramException("");
					}
					else
						me.FuxionBuilder.TypeKeyDirectory.Register(inter.GetGenericArguments()[0]);
				}
			}
			return me;
		}
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
			{
				foreach (var inter in handler.GetInterfaces().Where(t => t.IsSubclassOfRawGeneric(typeof(ICommandHandler<>))))
				{
					me.FuxionBuilder.Services.AddScoped(typeof(ICommandHandler<>).MakeGenericType(inter.GetGenericArguments()[0]), handler);
					me.FuxionBuilder.TypeKeyDirectory.Register(inter.GetGenericArguments()[0]);
				}
			}
			return me;
		}
	}
	public interface IFuxionBuilder
	{
		IServiceCollection Services { get; }
		TypeKeyDirectory TypeKeyDirectory { get; }
		void AddToAutoActivateList<T>();
	}

	internal class FuxionBuilder : IFuxionBuilder
	{
		public FuxionBuilder(IServiceCollection services, TypeKeyDirectory typeKeyDirectory)
		{
			Services = services;
			TypeKeyDirectory = typeKeyDirectory;
		}
		public IServiceCollection Services { get; }
		public TypeKeyDirectory TypeKeyDirectory { get; }

		public List<Type> AutoActivateList { get; } = new List<Type>();
		public void AddToAutoActivateList<T>() => AutoActivateList.Add(typeof(T));
	}
	public interface IEventsBuilder
	{
		IFuxionBuilder FuxionBuilder { get; }
	}

	internal class EventsBuilder : IEventsBuilder
	{
		public EventsBuilder(IFuxionBuilder fuxionBuilder) => FuxionBuilder = fuxionBuilder;
		public IFuxionBuilder FuxionBuilder { get; }
	}
	public interface ICommandsBuilder
	{
		IFuxionBuilder FuxionBuilder { get; }
	}

	internal class CommandsBuilder : ICommandsBuilder
	{
		public CommandsBuilder(IFuxionBuilder fuxionBuilder) => FuxionBuilder = fuxionBuilder;
		public IFuxionBuilder FuxionBuilder { get; }
	}
}
