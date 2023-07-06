using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks.Sources;
using Fuxion.Json;
using Fuxion.Reflection;

namespace Fuxion.Domain;

public interface INexus : IInitializable
{
	string NodeType { get; }
	string DeployId { get; set; }
	Guid InstanceId { get; }
	// NodeType - Identificador del tipo de nodo. Microservicio de Salto, Agente de Vaelsys, etc.
	// DeployId - Identificador de despliegue, OnPremise Molinero, Cloud Europa, Cloud Mexico, etc. 
	// InstanceId - Identificador de la instancia. Será lo mismo para los agentes, pero no para las clouds donde tenemos varias instanacias de un mismo servicio.
	
	// Tenant? - Quiza para fijar un nexo a un tenant? OnPremise tiene sentido, pero no sé si aporta algo

	
	// Producers - Productores de mensajes
	// IProducer Producer { get; }
	// Consumers - Consumidores de mensajes
	// IConsumer Consumer { get; }
	
	// Routes - Rutas por las que enviar y recibir mensajes
	// IReadOnlyCollection<IRoute<object,object>> Routes { get; }
	Task Publish<TMessage>(TMessage message) where TMessage : notnull;
	Task<IDisposable> OnReceive(Action<object> onMessageReceived);
}

public interface IInitializable
{
	Task Initialize();
}

public class Wuao(string _o)
{
	public void tt()
	{
		_o = "";
	}
}
public class DefaultNexus : INexus
{
	public DefaultNexus(string deployId)//, IProducer producer, IConsumer consumer, IList<IRoute<object,object>> routes)
	{
		DeployId = deployId;
		// Producer = producer;
		// Consumer = consumer;
		// Routes = new ReadOnlyCollection<IRoute<object,object>>(routes);
		// Producer.Attach(this);
		// Consumer.Attach(this);
		// foreach(var route in Routes)
		// 	route.Attach(this);
	}
	public async Task Initialize()
	{
		// await Producer.Initialize();
		// await Consumer.Initialize();
		// foreach (var route in Routes) await route.Initialize();
	}
	public string NodeType { get; } = "";
	public string DeployId { get; set; }
	public Guid InstanceId { get; } = Guid.NewGuid();
	// public IProducer Producer { get; }
	// public IConsumer Consumer { get; }
	// public IReadOnlyCollection<IRoute<object,object>> Routes { get; }
	
	
	public RouteDirectory RouteDirectory { get; } = new();
	// public void AddRoute<TSend, TReceive>(IRoute<TSend, TReceive> route)
	// {
	// 	RouteDirectory.AddPublisher<TSend>(new(""), route.Send);
	// }
	public async Task Publish<TMessage>(TMessage message)
		where TMessage : notnull
	{
		var publishers = RouteDirectory.GetPublishers<TMessage>();
		foreach (var publisher in publishers)
		{
			await publisher.Publish(message);
		}
	}
	public Task<IDisposable> OnReceive(Action<object> onMessageReceived)
	{
		return RouteDirectory.OnReceive(onMessageReceived);
	}
}

public class RouteDirectory
{
	Dictionary<Type, IEnumerable<IPublisher<object>>> publishers = new();
	public void AddPublisher<TMessage>(IPublisher<TMessage> publisher)
		where TMessage : notnull
	{
		publishers.Add(typeof(TMessage), new[]
		{
			// TODO Performance UNBOXING
			new BypassPublisher<object>(publisher.Info, message => publisher.Publish((TMessage)message))
		});
	}
	public void AddPublisher<TMessage>(PublisherInfo info, Func<TMessage, Task> publishFunc)
	{
		publishers.Add(typeof(TMessage),new[]
		{
			// TODO Performance UNBOXING
			new BypassPublisher<object>(info, message => publishFunc((TMessage)message))
		});
	}
	public IEnumerable<IPublisher<TMessage>> GetPublishers<TMessage>()
		where TMessage : notnull
	{
		// TODO Check if exist
		return publishers[typeof(TMessage)]
			.Select(p => new BypassPublisher<TMessage>(p.Info, m => p.Publish(m)));
	}

	List<ISubscriber<object>> subscribers = new();
	public void AddSubscriber<TMessage>(ISubscriber<TMessage> subscriber)
		where TMessage : notnull
	{
		subscribers.Add(new BypassSubscriber<object>(subscriber.Initialize, subscriber.Attach, action =>
		{
			return subscriber.OnReceive(msg =>
			{
				action(msg);
			});
		}));
	}
	public async Task<IDisposable> OnReceive(Action<object> onMessageReceived)
	{
		List<IDisposable> disposables = new();
		foreach (var subscriber in subscribers)
		{
			disposables.Add(await subscriber.OnReceive(onMessageReceived));
		}
		return disposables.AsDisposable(list =>
		{
			foreach (var dis in list) dis.Dispose();
		});
	}
}
public record PublisherInfo(string Name);
public interface IPublisher<in TMessage>
	where TMessage : notnull
{
	PublisherInfo Info { get; }
	Task Publish(TMessage message);
}

public class BypassPublisher<TMessage> : IPublisher<TMessage>
	where TMessage : notnull
{
	public BypassPublisher(PublisherInfo info, Func<TMessage, Task> onPublish)
	{
		Info = info;
		_onPublish = onPublish;
	}
	Func<TMessage, Task> _onPublish;
	public PublisherInfo Info { get; }
	public Task Publish(TMessage message) => _onPublish(message);
}

public interface ISubscriber<out TMessage>
	where TMessage : notnull
{
	Task Initialize();
	void Attach(INexus nexus);
	// Task Consume<TMessage>(TMessage message);
	// Func<object, bool> Receive { get; }
	Task<IDisposable> OnReceive(Action<TMessage> onMessageReceived);
	// TMessage Subscribe<TMessage>(Expression<Func<TMessage, bool>> predicate);
	// IObservable<TMessage> Observe<TMessage>(Expression<Func<TMessage, bool>> predicate);
}

public class BypassSubscriber<TMessage> : ISubscriber<TMessage>
	where TMessage : notnull
{
	public BypassSubscriber(Func<Task> onInitialize, Action<INexus> onAttach, Func<Action<TMessage>,Task<IDisposable>> onReceive)
	{
		_onInitialize = onInitialize;
		_onAttach = onAttach;
		_onReceive = onReceive;
	}
	Func<Task> _onInitialize;
	Action<INexus> _onAttach;
	Func<Action<TMessage>, Task<IDisposable>> _onReceive;
	public Task Initialize() => _onInitialize();
	public void Attach(INexus nexus) => _onAttach(nexus);
	public Task<IDisposable> OnReceive(Action<TMessage> onMessageReceived) => _onReceive(onMessageReceived);
}

public class ObservableSubscriberDecorator<TMessage> : ISubscriber<TMessage>
	where TMessage : notnull
{
	public ObservableSubscriberDecorator(ISubscriber<TMessage> innerSubscriber)
	{
		_innerSubscriber = innerSubscriber;
		_innerSubscriber.OnReceive(msg => subject.OnNext(msg));
	}
	ISubscriber<TMessage> _innerSubscriber;
	public Task Initialize() => _innerSubscriber.Initialize();
	public void Attach(INexus nexus) => _innerSubscriber.Attach(nexus);
	public Task<IDisposable> OnReceive(Action<TMessage> onMessageReceived) => _innerSubscriber.OnReceive(onMessageReceived);
	Subject<TMessage> subject = new();
	public IObservable<TMessage> Observe(Func<TMessage, bool> predicate)
	{
		return subject.Where(predicate);
	}
}

public class ObservableNexusDecorator
{
	public ObservableNexusDecorator(INexus nexus)
	{
		_nexus = nexus;
		_nexus.OnReceive(msg => subject.OnNext(msg));
	}
	INexus _nexus;
	// public Task<IDisposable> OnReceive(Action<object> onMessageReceived) => _nexus.OnReceive(onMessageReceived);
	Subject<object> subject = new();
	public IObservable<object> Observe(Func<object, bool> predicate)
	{
		return subject.Where(predicate);
	}
}

public static class ObservableNexusExtensions
{
	public static IObservable<object> Observe(this INexus nexus, Func<object, bool> predicate) => new ObservableNexusDecorator(nexus).Observe(predicate);
}

// public interface IObservableSubscriber : ISubscriber
// {
// 	Task<IDisposable> OnReceive<TMessage>(Action<TMessage> messageReceivedAction)
// 	{
// 		Observe(message => messageReceivedAction(message))
// 			.Subscribe()
// 	}
// 	IObservable<TMessage> Observe<TMessage>(Expression<Func<TMessage, bool>> predicate);
// }

// public static class ISubscriberExtensions
// {
// 	public static IObservable<TMessage> Observe<TMessage>(this ISubscriber<TMessage> me, Expression<Func<TMessage, bool>> predicate)
// 	{
// 		var t = me.Subscribe(predicate);
// 		me.OnReceive(()=>t);
// 		// me.OnReceive(message =>
// 		// {
// 		// 	// var t = me.Subscribe(predicate.Compile()
// 		// 	// 	.Invoke(message);
// 		// 	
// 		// });
// 	}
// }
public interface IMessage { }

// public interface IProducer
// {
// 	Task Initialize();
// 	void Attach(INexus nexus);
// 	Task Produce<TMessage>(TMessage message)
// 		where TMessage : notnull;
// }

// public class DefaultProducer : IProducer
// {
// 	INexus? _nexus;
// 	void IProducer.Attach(INexus nexus)
// 	{
// 		_nexus = nexus;
// 	}
// 	public Task Initialize() => Task.CompletedTask;
// 	public async Task Produce<TMessage>(TMessage message)
// 		where TMessage : notnull
// 	{
// 		if (_nexus is null) throw new InvalidStateException($"Producer was not attached in a node");
// 		var pod = new TypeKeyPod<TMessage>(message);
// 		RabbitMQRouteInfo rh = new()
// 		{
// 			Destination = "fuxion-lab-CL1-MS2"
// 		};
// 		pod.Headers.Add(new TypeKeyPod<RabbitMQRouteInfo>(rh));
// 		await _nexus.Routes.First()
// 			.Send(pod,rh);
// 	}
// 	// public async Task Produce(IMessage message)
// 	// {
// 	// 	if (_node is null) throw new InvalidStateException($"Producer was not attached in a node");
// 	// 	var pod = new TypeKeyPod<IMessage>(message);
// 	// 	pod.Headers.Add(new TypeKeyPod<RouteHeader>(new()
// 	// 	{
// 	// 		Destination = "fuxion-lab-CL1-MS2"
// 	// 	}));
// 	// 	await _node.Routes.First()
// 	// 		.Send(pod);
// 	// }
// }
// public interface IConsumer
// {
// 	Task Initialize();
// 	void Attach(INexus nexus);
// 	Task Consume<TMessage>(TMessage message);
// 	IObservable<TMessage> Observe<TMessage>(Expression<Func<TMessage, bool>> predicate);
// }

// public class DefaultConsumer : IConsumer
// {
// 	INexus? _node;
// 	public Task Initialize() => Task.CompletedTask;
// 	public void Attach(INexus nexus) => _node = nexus;
// 	public Task Consume<TMessage>(TMessage message)=> throw new NotImplementedException();
// 	public IObservable<TMessage> Observe<TMessage>(Expression<Func<TMessage, bool>> predicate) => throw new NotImplementedException();
// }

// public class a
// {
// 	public a()
// 	{
// 		Nexus2 n = new(new[]
// 		{
// 			new Route2()
// 		});
// 	}
// }
// public class Nexus2
// {
// 	public Nexus2(IList<IRoute2<object, object>> routes)
// 	{
// 		
// 	}
// }
//
// public class Route2 : IRoute2<string, string>
// {
// 	public void Attach(INexus nexus) => throw new NotImplementedException();
// 	public Task Initialize() => throw new NotImplementedException();
// 	public Task Send(string message) => throw new NotImplementedException();
// 	public Func<string, Task> Receive { get; set; }
// }
// public interface IRoute2<out TSend, out TReceive>
// {
// 	void Attach(INexus nexus);
// 	Task Initialize();
// 	IValueTaskSource<TSend> gg();
// 	Task Send(TSend message);
// 	Func<TReceive, Task> Receive { get; set; }
// }
public interface IRoute<in TSend, TReceive>
{
	void Attach(INexus nexus);
	Task Initialize();
	Task Send(TSend message);
	Func<TReceive, Task> Receive { get; set; }
	// Task Receive(TReceive message);
}
// https://medium.com/@bonnotguillaume/software-architecture-the-pipeline-design-pattern-from-zero-to-hero-b5c43d8a4e60
// public class RoutePipe<TSend, TReceive> : IRoute<TSend, TReceive>
// {
// 	ReadOnlyCollection<IRoute<object, object>> pipe = new(new List<IRoute<object, object>>());
// 	public void Attach(INexus nexus)
// 	{
// 		foreach(var item in pipe)item.Attach(nexus);
// 	}
// 	public async Task Initialize()
// 	{
// 		foreach(var item in pipe) await item.Initialize();
// 	}
// 	// Task IRoute<TMessage, TRouteInfo>.Send(TMessage message, TRouteInfo routeInfo) => Send(message, routeInfo);
// 	// public Task Receive(TReceive message) => throw new NotImplementedException();
// 	public Func<TReceive, Task> Receive { get; set; }
// 	public Task Send(TSend message) => throw new NotImplementedException();
// }

public abstract class RouteAdapter<TSendIn, TReceiveIn, TSendOut, TReceiveOut> : IRoute<TSendOut, TReceiveOut>
{
	internal IRoute<TSendIn, TReceiveIn> _route;
	public RouteAdapter(IRoute<TSendIn, TReceiveIn> route, Func<TReceiveOut, Task> receive)
	{
		Receive = receive;
		_route = route;
		_route.Receive = @out => Receive(ReceiveConverter(@out));
	}
	// public RouteAdapter(IRoute<TSendIn, TReceiveIn> route, Func<TReceiveIn, Task> receive)
	// {
	// 	// Receive = receive;
	// 	_route = route;
	// 	_route.Receive = receive;
	// 	Receive = @out => _route.Receive(ReceiveConverter(@out));
	// }
	public void Attach(INexus nexus) => _route.Attach(nexus);
	public Task Initialize() => _route.Initialize();
	public Task Send(TSendOut message) => _route.Send(SendConverter(message));
	public Func<TReceiveOut, Task> Receive { get; set; }
	protected abstract TSendIn SendConverter(TSendOut message);
	protected abstract TReceiveOut ReceiveConverter(TReceiveIn message);
}

public class ObjectRouteAdapter<TSendIn, TReceiveIn> : RouteAdapter<UriKeyPod<IMessage>, UriKeyPod<IMessage>,object, object>
	where TReceiveIn : class
{
	public ObjectRouteAdapter(RouteAdapter<TSendIn, TReceiveIn, UriKeyPod<IMessage>, UriKeyPod<IMessage>> adapter)
		: base(adapter, obj => adapter.Receive((UriKeyPod<IMessage>)obj)) { }
	protected override UriKeyPod<IMessage> SendConverter(object message) => (UriKeyPod<IMessage>)message;
	protected override object ReceiveConverter(UriKeyPod<IMessage> message) => message;
}