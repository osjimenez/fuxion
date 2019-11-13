using Fuxion.Reflection;
using Fuxion.Application;
using Fuxion.Application.Events;
using Fuxion.Domain;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.RabbitMQ
{
	public class RabbitMQEventBus : IEventPublisher, IEventSubscriber
	{
		public RabbitMQEventBus(
			IServiceProvider serviceProvider,
			IRabbitMQPersistentConnection persistentConnection,
			TypeKeyDirectory typeKeyDirectory,
			string exchangeName,
			string queueName,
			int retryCount)
		{
			this.serviceProvider = serviceProvider;
			this.persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
			this.typeKeyDirectory = typeKeyDirectory;
			this.exchangeName = exchangeName;
			this.queueName = queueName;
			this.retryCount = retryCount;
			_consumerChannel = CreateConsumerChannel();
		}
		private readonly IRabbitMQPersistentConnection persistentConnection;
		private readonly TypeKeyDirectory typeKeyDirectory;
		private readonly string exchangeName;
		private readonly string queueName;
		private readonly int retryCount;
		private IModel _consumerChannel;
		private readonly IServiceProvider serviceProvider;

		private IModel CreateConsumerChannel()
		{
			Debug.WriteLine("============================================");

			if (!persistentConnection.IsConnected)
			{
				persistentConnection.TryConnect();
			}

			var channel = persistentConnection.CreateModel();

			channel.ExchangeDeclare(exchange: exchangeName,
								 type: "direct");

			channel.QueueDeclare(queue: queueName,
								 durable: true,
								 exclusive: false,
								 autoDelete: false,
								 arguments: null);


			var consumer = new EventingBasicConsumer(channel);
			consumer.Received += async (model, ea) =>
			{
				//var integrationEventTypeId = ea.RoutingKey;
				var message = Encoding.UTF8.GetString(ea.Body);
				var pod = message.FromJson<PublicationPod>();
				var @event = pod.WithTypeKeyDirectory(typeKeyDirectory);
				if (@event == null) throw new InvalidCastException($"Event with key '{pod.PayloadKey}' is not registered in '{nameof(TypeKeyDirectory)}'");
				await ProcessEvent(@event);
				//await ProcessEvent(integrationEventTypeId, message);
				//await ProcessEvent(message.FromJson<IEvent>());

				channel.BasicAck(ea.DeliveryTag, multiple: false);
			};

			channel.BasicConsume(queue: queueName,
								 autoAck: false,
								 consumer: consumer);

			channel.CallbackException += (sender, ea) =>
			{
				_consumerChannel.Dispose();
				_consumerChannel = CreateConsumerChannel();
			};

			return channel;
		}
		private async Task ProcessEvent(Event @event)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				//var handlers = scope.ServiceProvider.GetServices(typeof(IEnumerable<>).MakeGenericType(typeof(IEventHandler<>).MakeGenericType(@event.GetType())));
				var handlers = scope.ServiceProvider.GetServices(typeof(IEventHandler<>).MakeGenericType(@event.GetType()));
				IEventHandler<Event> c;
				foreach (var handler in handlers)
					await (Task)handler.GetType().GetMethod(nameof(c.HandleAsync)).Invoke(handler, new object[] { @event });
			}
		}
		public async Task PublishAsync(Event @event)
		{
			// TODO - Es aqui donde se debe poner la feature de publicacion ?? no lo sé
			@event.AddPublication(DateTime.UtcNow);
			if (!persistentConnection.IsConnected)
			{
				persistentConnection.TryConnect();
			}

			var policy = RetryPolicy.Handle<BrokerUnreachableException>()

				.Or<SocketException>()
				.WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(System.Math.Pow(2, retryAttempt)), (ex, time) =>
				{
					Debug.WriteLine(ex.ToString());
					//_logger.LogWarning(ex.ToString());
				});

			using (var channel = persistentConnection.CreateModel())
			{
				//var eventTypeId = @event.IntegrationEventTypeKey;
				channel.ExchangeDeclare(exchange: exchangeName,
									type: "direct");
				var eventPod = @event.ToPublicationPod();
				var message = JsonConvert.SerializeObject(eventPod);
				var body = Encoding.UTF8.GetBytes(message);
				await policy.ExecuteAsync(() =>
				{
					var properties = channel.CreateBasicProperties();
					properties.DeliveryMode = 2; // persistent

					channel.BasicPublish(exchange: exchangeName,
									 routingKey: eventPod.PayloadKey,
									 mandatory: true,
									 basicProperties: properties,
									 body: body);
					return Task.CompletedTask;
				});
			}
		}

		public void Subscribe<TEvent>() where TEvent : Event => Subscribe(typeof(TEvent).GetTypeKey());
		public void Subscribe(Type type) => Subscribe(type.GetTypeKey());

		private void Subscribe(string eventTypeKey)
		{
			if (!persistentConnection.IsConnected && !persistentConnection.TryConnect())
				throw new RabbitMQConnectionException("Cannot connect to Rabbit MQ");
			using (var channel = persistentConnection.CreateModel())
			{
				channel.QueueBind(queue: queueName,
								  exchange: exchangeName,
								  routingKey: eventTypeKey);
			}
		}
	}
}
