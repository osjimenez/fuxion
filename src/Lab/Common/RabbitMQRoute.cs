using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using Fuxion.Domain;
using Fuxion.Domain.Plugin;
using Fuxion.Json;
using Fuxion.Reflection;
using Fuxion.Threading;
using Fuxion.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using IMessage = Fuxion.Domain.IMessage;

namespace Fuxion.Lab.Common;

// public class RabbitMQRouteAdapter : RouteAdapter<RabbitMQSend, RabbitMQReceive,UriKeyPod<IMessage>, UriKeyPod<IMessage>>
// {
// 	public RabbitMQRouteAdapter(RabbitMQRoute route, Func<UriKeyPod<IMessage>, Task> receive) : base(route, receive) { }
// 	protected override RabbitMQSend SendConverter(UriKeyPod<IMessage> message)
// 	{
// 		if (!message.Headers.Has(typeof(RabbitMQSend).GetUriKey())) throw new UriKeyNotFoundException($"Header to routing not found");
// 		return new()
// 		{
// 			RoutingKey = ((RabbitMQSend)message.Headers[typeof(RabbitMQSend).GetUriKey()].Outside()).RoutingKey,
// 			Body = Encoding.UTF8.GetBytes(message.SerializeToJson())
// 		};
// 	}
// 	protected override UriKeyPod<IMessage> ReceiveConverter(RabbitMQReceive message)
// 		=> Encoding.UTF8.GetString(message.Body.ToArray())
// 			.DeserializeFromJson<UriKeyPod<IMessage>>() ?? throw new SerializationException($"Deserialization was null");
// }

public class RabbitMQPublisher : IPublisher<RabbitMQSend>
{
	RabbitMQConnection _connection;
	IOptions<RabbitSettings> _settings;
	ILogger<RabbitMQPublisher> _logger;
	public RabbitMQPublisher(RabbitMQConnection connection, IOptions<RabbitSettings> settings, ILogger<RabbitMQPublisher> logger)
	{
		_connection = connection;
		_settings = settings;
		_logger = logger;
		Info = new("");
	}
	public PublisherInfo Info { get; }
	public async Task Publish(RabbitMQSend message) => await _connection.DoWithConnection(async connection =>
	{
		var settings = _settings.Value;
		var policy = Policy.Handle<BrokerUnreachableException>()
			.Or<SocketException>()
			.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(System.Math.Pow(2, retryAttempt)), (ex, time) =>
			{
				_logger.LogWarning($"Error '{ex.GetType().Name}' on send: {ex.Message}");
			});
		await policy.ExecuteAsync(async () =>
		{
			await using var channel = await connection.CreateChannelAsync();
			await channel.ExchangeDeclareAsync(settings.Exchange, ExchangeType.Direct);
			var properties = new BasicProperties
			{
				DeliveryMode = DeliveryModes.Persistent
			};
			await channel.BasicPublishAsync(settings.Exchange, message.RoutingKey, true, properties, message.Body);
			return Task.CompletedTask;
		});
	});
}
public class RabbitMQSubscriber(
	RabbitMQConnection connection, 
	ILogger<RabbitMQSubscriber> logger, 
	IOptions<RabbitSettings> settings,
	IServiceProvider serviceProvider) : ISubscriber<RabbitMQReceive>
{
	INexus? nexus;
	IChannel? consumerChannel;
	public async Task Initialize()
	{
		consumerChannel = await CreateConsumerChannel();
	}
	public void Attach(INexus nexus) => this.nexus = nexus;
	readonly List<Action<IReceipt<RabbitMQReceive>>> receivers = [];
	public Task<IDisposable> OnReceive(Action<IReceipt<RabbitMQReceive>> onMessageReceived)
	{
		receivers.Add(onMessageReceived);
		var dis = onMessageReceived.AsDisposable(f =>
		{
			receivers.Remove(f);
		});
		return Task.FromResult<IDisposable>(dis);
	}
	public async Task<IChannel> CreateConsumerChannel()
		=> await connection.DoWithConnection(async connection =>
		{
			// TODO Change exception type
			if (nexus is null) throw new InvalidStateException($"Route was not attached to node.");
			logger.LogInformation("Creating consumer channel ...");
			// if (!(_connection is { IsOpen: true } && !_disposed)) Connect();
			// if (_connection is null) throw new InvalidProgramException($"Connection was null at create consumer channel");
			var settings1 = settings.Value;
			var channel = await connection.CreateChannelAsync();
			await channel.ExchangeDeclareAsync(settings1.Exchange, ExchangeType.Direct);
			await channel.QueueDeclareAsync(settings1.QueuePrefix + nexus.DeployId, true, false, false, null);
			//channel.QueueBindAsync();
			var consumer = new AsyncEventingBasicConsumer(channel);
			consumer.ReceivedAsync += async (model, ea) =>
			{
				using var scope = serviceProvider.CreateScope();
				foreach (var receive in receivers)
				{
					receive(new Receipt<RabbitMQReceive>(new()
					{
						Body = ea.Body,
						Source = ""
					}, scope.ServiceProvider));
				}
				await channel.BasicAckAsync(ea.DeliveryTag, false);
			};
			await channel.BasicConsumeAsync(settings1.QueuePrefix + nexus.DeployId, false, consumer);
			channel.CallbackExceptionAsync += async (sender, ea) =>
			{
				consumerChannel?.DisposeAsync();
				consumerChannel = await CreateConsumerChannel();
			};
			await channel.QueueBindAsync(settings1.QueuePrefix + nexus.DeployId, settings1.Exchange, settings1.QueuePrefix + nexus.DeployId, new Dictionary<string, object?>());
			return channel;
		});
}

public class RabbitMQConnection : IDisposable
{
	readonly IOptions<RabbitSettings> _settings;
	ILogger<RabbitMQConnection> _logger;
	IConnection? _connection;
	readonly object _sync_root = new();
	int _retryCount = 5;
	public RabbitMQConnection(IOptions<RabbitSettings> settings, ILogger<RabbitMQConnection> logger)
	{
		_settings = settings;
		_logger = logger;
		Receive = null!;
	}
	public async Task Initialize()
	{
		_connection = await Connect();
	}
	public Func<RabbitMQReceive, Task> Receive { get; set; }
	bool _disposed;
	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		try
		{
			_connection?.Dispose();
		} catch (IOException ex)
		{
			_logger.LogCritical(ex, ex.Message);
		}
	}
	async Task<IConnection> Connect()
	{
		lock (_sync_root)
		{
			if (_connection is { IsOpen: true } && !_disposed) return _connection;
			_logger.LogInformation("RabbitMQ Client is trying to connect");
			var policy = Policy.Handle<SocketException>()
				.Or<BrokerUnreachableException>()
				.WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(System.Math.Pow(2, retryAttempt)),
					(ex, time) => _logger.LogWarning(ex, $"Error connecting RabbitMQ '{ex.Message}', retrying ..."));
			var settings = _settings.Value;
			IConnection? res = null;
			policy.Execute(() => { 
				res = new ConnectionFactory
				{
					HostName = settings.Host,
					Port = settings.Port
				}.CreateConnectionAsync().Result;
			});
			if (res is { IsOpen: true })
			{
				res.ConnectionShutdownAsync += async (s, e) =>
				{
					if (_disposed) return;
					_logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");
					_connection = await Connect();
				};
				res.CallbackExceptionAsync += async (s, e) =>
				{
					if (_disposed) return;
					_logger.LogWarning($"A RabbitMQ connection throw '{e.Exception.GetType().Name}'. Trying to re-connect...");
					_connection = await Connect();
				};
				res.ConnectionBlockedAsync += async (s, e) =>
				{
					if (_disposed) return;
					_logger.LogWarning("A RabbitMQ connection was blocked. Trying to re-connect...");
					_connection = await Connect();
				};
				_logger.LogInformation($"RabbitMQ persistent connection acquired a connection '{res.Endpoint.HostName}' and is subscribed to failure events");
				return res;
			}
			_logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
			// TODO Change by RabbitMQConnectionException in Fuxion.RabbitMQ project
			throw new Exception("RabbitMQ connection failed");
		}
	}
	public async ValueTask DoWithConnection(Func<IConnection, Task> action)
	{
		if (_connection is { IsOpen: true } && !_disposed) await action(_connection);
		else
		{
			_connection = await Connect();
			await action(_connection);
		}
	}
	public async ValueTask<TResult> DoWithConnection<TResult>(Func<IConnection, Task<TResult>> function)
	{
		if (_connection is { IsOpen: true } && !_disposed) return await function(_connection);
		_connection = await Connect();
		return await function(_connection);
	}
}
//public class RabbitMQRoute : IRoute<RabbitMQSend ,RabbitMQReceive>, IDisposable
//{
//	readonly IOptions<RabbitSettings> _settings;
//	ILogger<RabbitMQRoute> _logger;
//	IConnection? _connection;
//	readonly object _sync_root = new();
//	int _retryCount = 5;
//	IChannel? _consumerChannel;
//	INexus? _nexus;
//	public RabbitMQRoute(IOptions<RabbitSettings> settings, ILogger<RabbitMQRoute> logger)
//	{
//		_settings = settings;
//		_logger = logger;
//		Receive = null!;
//	}
//	public void Attach(INexus nexus) => _nexus = nexus;
//	public Task Initialize()
//	{
//		_consumerChannel = CreateConsumerChannel();
//		return Task.CompletedTask;
//	}
//	public Task Send(RabbitMQSend  message)
//		=> DoWithConnection(async connection =>
//		{
//			var settings = _settings.Value;
//			var policy = Policy.Handle<BrokerUnreachableException>()
//				.Or<SocketException>()
//				.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(System.Math.Pow(2, retryAttempt)), (ex, time) =>
//				{
//					_logger.LogWarning($"Error '{ex.GetType().Name}' on send: {ex.Message}");
//				});
//			await policy.ExecuteAsync(() =>
//			{
//				using var channel = connection.CreateModel();
//				channel.ExchangeDeclare(settings.Exchange, ExchangeType.Direct);
//				var properties = channel.CreateBasicProperties();
//				properties.DeliveryMode = 2; // persistent
//				channel.BasicPublish(settings.Exchange, message.RoutingKey, true, properties, message.Body);
//				return Task.CompletedTask;
//			});
//		});
//	public Func<RabbitMQReceive, Task> Receive { get; set; }
//	bool _disposed;
//	public void Dispose()
//	{
//		if (_disposed) return;
//		_disposed = true;
//		try
//		{
//			_connection?.Dispose();
//		} catch (IOException ex)
//		{
//			_logger.LogCritical(ex, ex.Message);
//		}
//	}
//	public IConnection Connect()
//	{
//		_logger.LogInformation("RabbitMQ Client is trying to connect");
//		lock (_sync_root)
//		{
//			if (_connection is { IsOpen: true } && !_disposed) return _connection;
//			var policy = Policy.Handle<SocketException>()
//				.Or<BrokerUnreachableException>()
//				.WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(System.Math.Pow(2, retryAttempt)),
//					(ex, time) => _logger.LogWarning(ex, $"Error connecting RabbitMQ '{ex.Message}', retrying ..."));
//			var settings = _settings.Value;
//			IConnection? res = null;
//			policy.Execute(() => { 
//				res = new ConnectionFactory
//				{
//					HostName = settings.Host,
//					Port = settings.Port
//				}.CreateConnection();
//			});
//			if (res is { IsOpen: true })
//			{
//				res.ConnectionShutdown += (s, e) =>
//				{
//					if (_disposed) return;
//					_logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");
//					_connection = Connect();
//				};
//				res.CallbackException += (s, e) =>
//				{
//					if (_disposed) return;
//					_logger.LogWarning($"A RabbitMQ connection throw '{e.Exception.GetType().Name}'. Trying to re-connect...");
//					_connection = Connect();
//				};
//				res.ConnectionBlocked += (s, e) =>
//				{
//					if (_disposed) return;
//					_logger.LogWarning("A RabbitMQ connection was blocked. Trying to re-connect...");
//					_connection = Connect();
//				};
//				_logger.LogInformation($"RabbitMQ persistent connection acquired a connection '{res.Endpoint.HostName}' and is subscribed to failure events");
//				return res;
//			}
//			_logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
//			// TODO Change by RabbitMQConnectionException in Fuxion.RabbitMQ project
//			throw new Exception("RabbitMQ connection failed");
//		}
//	}
//	void DoWithConnection(Action<IConnection> action)
//	{
//		if (_connection is { IsOpen: true } && !_disposed) action(_connection);
//		else
//		{
//			_connection = Connect();
//			action(_connection);
//		}
//	}
//	TResult DoWithConnection<TResult>(Func<IConnection, TResult> function)
//	{
//		if (_connection is { IsOpen: true } && !_disposed) return function(_connection);
//		else
//		{
//			_connection = Connect();
//			return function(_connection);
//		}
//	}
//	public IModel CreateConsumerChannel()
//		=> DoWithConnection(connection =>
//		{
//			if (_nexus is null) throw new InvalidStateException($"Route was not attached to node.");
//			_logger.LogInformation("Creating consumer channel ...");
//			// if (!(_connection is { IsOpen: true } && !_disposed)) Connect();
//			// if (_connection is null) throw new InvalidProgramException($"Connection was null at create consumer channel");
//			var settings = _settings.Value;
//			var channel = connection.CreateModel();
//			channel.ExchangeDeclare(settings.Exchange, ExchangeType.Direct);
//			channel.QueueDeclare(settings.QueuePrefix + _nexus.DeployId, true, false, false, null);
//			var consumer = new EventingBasicConsumer(channel);
//			consumer.Received += async (model, ea) =>
//			{
//				await Receive(new()
//				{
//					Body = ea.Body,
//					Source = ""
//				});
//				channel.BasicAck(ea.DeliveryTag, false);
//			};
//			channel.BasicConsume(settings.QueuePrefix + _nexus.DeployId, false, consumer);
//			channel.CallbackException += (sender, ea) =>
//			{
//				_consumerChannel?.Dispose();
//				_consumerChannel = CreateConsumerChannel();
//			};
//			channel.QueueBind(settings.QueuePrefix + _nexus.DeployId, settings.Exchange, settings.QueuePrefix + _nexus.DeployId, new Dictionary<string, object>());
//			return channel;
//		});
//}