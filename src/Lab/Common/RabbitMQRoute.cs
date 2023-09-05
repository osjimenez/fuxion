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
	public Task Publish(RabbitMQSend message) => _connection.DoWithConnection(async connection =>
	{
		var settings = _settings.Value;
		var policy = Policy.Handle<BrokerUnreachableException>()
			.Or<SocketException>()
			.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(System.Math.Pow(2, retryAttempt)), (ex, time) =>
			{
				_logger.LogWarning($"Error '{ex.GetType().Name}' on send: {ex.Message}");
			});
		await policy.ExecuteAsync(() =>
		{
			using var channel = connection.CreateModel();
			channel.ExchangeDeclare(settings.Exchange, ExchangeType.Direct);
			var properties = channel.CreateBasicProperties();
			properties.DeliveryMode = 2; // persistent
			channel.BasicPublish(settings.Exchange, message.RoutingKey, true, properties, message.Body);
			return Task.CompletedTask;
		});
	});
}
public class RabbitMQSubscriber : ISubscriber<RabbitMQReceive>
{
	RabbitMQConnection _connection;
	ILogger<RabbitMQSubscriber> _logger;
	IOptions<RabbitSettings> _settings;
	INexus? _nexus;
	IModel? _consumerChannel;
	public Task Initialize()
	{
		_consumerChannel = CreateConsumerChannel();
		return Task.CompletedTask;
	}
	public void Attach(INexus nexus) => _nexus = nexus;
	List<Action<RabbitMQReceive>> receivers = new();
	public RabbitMQSubscriber(RabbitMQConnection connection, ILogger<RabbitMQSubscriber> logger, IOptions<RabbitSettings> settings)
	{
		_connection = connection;
		_logger = logger;
		_settings = settings;
	}
	public Task<IDisposable> OnReceive(Action<RabbitMQReceive> onMessageReceived)
	{
		receivers.Add(onMessageReceived);
		var dis = onMessageReceived.AsDisposable(f =>
		{
			receivers.Remove(f);
		});
		return Task.FromResult<IDisposable>(dis);
	}
	public IModel CreateConsumerChannel()
		=> _connection.DoWithConnection(connection =>
		{
			// TODO Change exception type
			if (_nexus is null) throw new InvalidStateException($"Route was not attached to node.");
			_logger.LogInformation("Creating consumer channel ...");
			// if (!(_connection is { IsOpen: true } && !_disposed)) Connect();
			// if (_connection is null) throw new InvalidProgramException($"Connection was null at create consumer channel");
			var settings = _settings.Value;
			var channel = connection.CreateModel();
			channel.ExchangeDeclare(settings.Exchange, ExchangeType.Direct);
			channel.QueueDeclare(settings.QueuePrefix + _nexus.DeployId, true, false, false, null);
			var consumer = new EventingBasicConsumer(channel);
			consumer.Received += (model, ea) =>
			{
				foreach (var receive in receivers)
				{
					receive(new()
					{
						Body = ea.Body,
						Source = ""
					});
				}
				channel.BasicAck(ea.DeliveryTag, false);
			};
			channel.BasicConsume(settings.QueuePrefix + _nexus.DeployId, false, consumer);
			channel.CallbackException += (sender, ea) =>
			{
				_consumerChannel?.Dispose();
				_consumerChannel = CreateConsumerChannel();
			};
			channel.QueueBind(settings.QueuePrefix + _nexus.DeployId, settings.Exchange, settings.QueuePrefix + _nexus.DeployId, new Dictionary<string, object>());
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
	public Task Initialize()
	{
		_connection = Connect();
		return Task.CompletedTask;
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
	IConnection Connect()
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
				}.CreateConnection();
			});
			if (res is { IsOpen: true })
			{
				res.ConnectionShutdown += (s, e) =>
				{
					if (_disposed) return;
					_logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");
					_connection = Connect();
				};
				res.CallbackException += (s, e) =>
				{
					if (_disposed) return;
					_logger.LogWarning($"A RabbitMQ connection throw '{e.Exception.GetType().Name}'. Trying to re-connect...");
					_connection = Connect();
				};
				res.ConnectionBlocked += (s, e) =>
				{
					if (_disposed) return;
					_logger.LogWarning("A RabbitMQ connection was blocked. Trying to re-connect...");
					_connection = Connect();
				};
				_logger.LogInformation($"RabbitMQ persistent connection acquired a connection '{res.Endpoint.HostName}' and is subscribed to failure events");
				return res;
			}
			_logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
			// TODO Change by RabbitMQConnectionException in Fuxion.RabbitMQ project
			throw new Exception("RabbitMQ connection failed");
		}
	}
	public void DoWithConnection(Action<IConnection> action)
	{
		if (_connection is { IsOpen: true } && !_disposed) action(_connection);
		else
		{
			_connection = Connect();
			action(_connection);
		}
	}
	public TResult DoWithConnection<TResult>(Func<IConnection, TResult> function)
	{
		if (_connection is { IsOpen: true } && !_disposed) return function(_connection);
		else
		{
			_connection = Connect();
			return function(_connection);
		}
	}
}
public class RabbitMQRoute : IRoute<RabbitMQSend ,RabbitMQReceive>, IDisposable
{
	readonly IOptions<RabbitSettings> _settings;
	ILogger<RabbitMQRoute> _logger;
	IConnection? _connection;
	readonly object _sync_root = new();
	int _retryCount = 5;
	IModel? _consumerChannel;
	INexus? _nexus;
	public RabbitMQRoute(IOptions<RabbitSettings> settings, ILogger<RabbitMQRoute> logger)
	{
		_settings = settings;
		_logger = logger;
		Receive = null!;
	}
	public void Attach(INexus nexus) => _nexus = nexus;
	public Task Initialize()
	{
		_consumerChannel = CreateConsumerChannel();
		return Task.CompletedTask;
	}
	public Task Send(RabbitMQSend  message)
		=> DoWithConnection(async connection =>
		{
			var settings = _settings.Value;
			var policy = Policy.Handle<BrokerUnreachableException>()
				.Or<SocketException>()
				.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(System.Math.Pow(2, retryAttempt)), (ex, time) =>
				{
					_logger.LogWarning($"Error '{ex.GetType().Name}' on send: {ex.Message}");
				});
			await policy.ExecuteAsync(() =>
			{
				using var channel = connection.CreateModel();
				channel.ExchangeDeclare(settings.Exchange, ExchangeType.Direct);
				var properties = channel.CreateBasicProperties();
				properties.DeliveryMode = 2; // persistent
				channel.BasicPublish(settings.Exchange, message.RoutingKey, true, properties, message.Body);
				return Task.CompletedTask;
			});
		});
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
	public IConnection Connect()
	{
		_logger.LogInformation("RabbitMQ Client is trying to connect");
		lock (_sync_root)
		{
			if (_connection is { IsOpen: true } && !_disposed) return _connection;
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
				}.CreateConnection();
			});
			if (res is { IsOpen: true })
			{
				res.ConnectionShutdown += (s, e) =>
				{
					if (_disposed) return;
					_logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");
					_connection = Connect();
				};
				res.CallbackException += (s, e) =>
				{
					if (_disposed) return;
					_logger.LogWarning($"A RabbitMQ connection throw '{e.Exception.GetType().Name}'. Trying to re-connect...");
					_connection = Connect();
				};
				res.ConnectionBlocked += (s, e) =>
				{
					if (_disposed) return;
					_logger.LogWarning("A RabbitMQ connection was blocked. Trying to re-connect...");
					_connection = Connect();
				};
				_logger.LogInformation($"RabbitMQ persistent connection acquired a connection '{res.Endpoint.HostName}' and is subscribed to failure events");
				return res;
			}
			_logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
			// TODO Change by RabbitMQConnectionException in Fuxion.RabbitMQ project
			throw new Exception("RabbitMQ connection failed");
		}
	}
	void DoWithConnection(Action<IConnection> action)
	{
		if (_connection is { IsOpen: true } && !_disposed) action(_connection);
		else
		{
			_connection = Connect();
			action(_connection);
		}
	}
	TResult DoWithConnection<TResult>(Func<IConnection, TResult> function)
	{
		if (_connection is { IsOpen: true } && !_disposed) return function(_connection);
		else
		{
			_connection = Connect();
			return function(_connection);
		}
	}
	public IModel CreateConsumerChannel()
		=> DoWithConnection(connection =>
		{
			if (_nexus is null) throw new InvalidStateException($"Route was not attached to node.");
			_logger.LogInformation("Creating consumer channel ...");
			// if (!(_connection is { IsOpen: true } && !_disposed)) Connect();
			// if (_connection is null) throw new InvalidProgramException($"Connection was null at create consumer channel");
			var settings = _settings.Value;
			var channel = connection.CreateModel();
			channel.ExchangeDeclare(settings.Exchange, ExchangeType.Direct);
			channel.QueueDeclare(settings.QueuePrefix + _nexus.DeployId, true, false, false, null);
			var consumer = new EventingBasicConsumer(channel);
			consumer.Received += async (model, ea) =>
			{
				await Receive(new()
				{
					Body = ea.Body,
					Source = ""
				});
				channel.BasicAck(ea.DeliveryTag, false);
			};
			channel.BasicConsume(settings.QueuePrefix + _nexus.DeployId, false, consumer);
			channel.CallbackException += (sender, ea) =>
			{
				_consumerChannel?.Dispose();
				_consumerChannel = CreateConsumerChannel();
			};
			channel.QueueBind(settings.QueuePrefix + _nexus.DeployId, settings.Exchange, settings.QueuePrefix + _nexus.DeployId, new Dictionary<string, object>());
			return channel;
		});
}