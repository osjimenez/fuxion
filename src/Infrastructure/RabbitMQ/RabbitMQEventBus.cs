using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using Fuxion.Application;
using Fuxion.Application.Events;
using Fuxion.Domain;
using Fuxion.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Fuxion.RabbitMQ;

public class RabbitMQEventBus : IEventPublisher, IEventSubscriber
{
	public RabbitMQEventBus(IServiceProvider serviceProvider,
		IRabbitMQPersistentConnection persistentConnection,
		ITypeKeyResolver typeKeyResolver,
		string exchangeName,
		string queueName,
		int retryCount)
	{
		this.serviceProvider = serviceProvider;
		this.persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
		_typeKeyResolver = typeKeyResolver;
		this.exchangeName = exchangeName;
		this.queueName = queueName;
		this.retryCount = retryCount;
		_consumerChannel = CreateConsumerChannel();
	}
	readonly string exchangeName;
	readonly IRabbitMQPersistentConnection persistentConnection;
	readonly string queueName;
	readonly int retryCount;
	readonly IServiceProvider serviceProvider;
	readonly ITypeKeyResolver _typeKeyResolver;
	IModel _consumerChannel;
	public async Task PublishAsync(Event @event)
	{
		//TODO - Es aqui donde se debe poner la feature de publicacion ?? no lo sé
		@event.AddPublication(DateTime.UtcNow);
		if (!persistentConnection.IsConnected) persistentConnection.TryConnect();
		var policy = Policy.Handle<BrokerUnreachableException>().Or<SocketException>().WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(System.Math.Pow(2, retryAttempt)),
			(ex, time) => {
				Debug.WriteLine(ex.ToString());
				//_logger.LogWarning(ex.ToString());
			});
		using (var channel = persistentConnection.CreateModel())
		{
			//var eventTypeId = @event.IntegrationEventTypeKey;
			channel.ExchangeDeclare(exchangeName, "direct");
			var eventPod = @event.ToPublicationPod();
			//TODO - Comprobar que funciona este cambio
			//var message = JsonConvert.SerializeObject(eventPod);
			var message = eventPod.ToJson();
			var body = Encoding.UTF8.GetBytes(message);
			await policy.ExecuteAsync(() => {
				var properties = channel.CreateBasicProperties();
				properties.DeliveryMode = 2; // persistent
				channel.BasicPublish(exchangeName, eventPod.Discriminator.ToString(), true, properties, body);
				return Task.CompletedTask;
			});
		}
	}
	public void Subscribe<TEvent>() where TEvent : Event => Subscribe(typeof(TEvent).GetTypeKey());
	IModel CreateConsumerChannel()
	{
		Debug.WriteLine("============================================");
		if (!persistentConnection.IsConnected) persistentConnection.TryConnect();
		var channel = persistentConnection.CreateModel();
		channel.ExchangeDeclare(exchangeName, "direct");
		channel.QueueDeclare(queueName, true, false, false, null);
		var consumer = new EventingBasicConsumer(channel);
		consumer.Received += async (model, ea) => {
			//var integrationEventTypeId = ea.RoutingKey;
			var message = Encoding.UTF8.GetString(ea.Body.ToArray());
			var pod = message.DeserializeFromJson<PublicationPod>(true);
			var @event  = pod.WithTypeKeyResolver(_typeKeyResolver);
			if (@event == null) throw new InvalidCastException($"Event with discriminator '{pod.Discriminator}' is not registered in '{nameof(TypeKeyDirectory)}'");
			await ProcessEvent(@event);
			//await ProcessEvent(integrationEventTypeId, message);
			//await ProcessEvent(message.FromJson<IEvent>());
			channel.BasicAck(ea.DeliveryTag, false);
		};
		channel.BasicConsume(queueName, false, consumer);
		channel.CallbackException += (sender, ea) => {
			_consumerChannel.Dispose();
			_consumerChannel = CreateConsumerChannel();
		};
		return channel;
	}
	async Task ProcessEvent(Event @event)
	{
		using var scope = serviceProvider.CreateScope();
		//var handlers = scope.ServiceProvider.GetServices(typeof(IEnumerable<>).MakeGenericType(typeof(IEventHandler<>).MakeGenericType(@event.GetType())));
		var handlers = scope.ServiceProvider.GetServices(typeof(IEventHandler<>).MakeGenericType(@event.GetType()));
		IEventHandler<Event> c;
		foreach (var handler in handlers)
		{
			var res = handler?.GetType().GetMethod(nameof(c.HandleAsync))?.Invoke(handler, new object[] {
				@event
			});
			if (res is not null) await (Task)res;
		}
	}
	public void Subscribe(Type type) => Subscribe(type.GetTypeKey());
	void Subscribe(TypeKey eventTypeKey)
	{
		if (!persistentConnection.IsConnected && !persistentConnection.TryConnect()) throw new RabbitMQConnectionException("Cannot connect to Rabbit MQ");
		using (var channel = persistentConnection.CreateModel()) channel.QueueBind(queueName, exchangeName, eventTypeKey.ToString());
	}
}