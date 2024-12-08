using System.Diagnostics;
using System.Net.Sockets;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Fuxion.Lab.Common;

public class DefaultRabbitMQPersistentConnection : IRabbitMQPersistentConnection
{
	//public DefaultRabbitMQPersistentConnection(IConnectionFactory connectionFactory, ILogger<DefaultRabbitMQPersistentConnection> logger, int retryCount = 5)
	public DefaultRabbitMQPersistentConnection(IConnectionFactory connectionFactory, int retryCount = 5)
	{
		_connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
		//_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_retryCount = retryCount;
	}
	readonly IConnectionFactory _connectionFactory;
	//private readonly ILogger<DefaultRabbitMQPersistentConnection> _logger;
	readonly int _retryCount;
	readonly object sync_root = new();
	IConnection? _connection;
	bool _disposed;
	public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;
	public async Task<IChannel> CreateModel()
	{
		if (!IsConnected) throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
		return await _connection!.CreateChannelAsync();
	}
	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;
		try
		{
			_connection?.Dispose();
		} catch (IOException ex)
		{
			Debug.WriteLine(ex.ToString());
			//_logger.LogCritical(ex.ToString());
		}
	}
	public Task<bool> TryConnect()
	{
		Debug.WriteLine("RabbitMQ Client is trying to connect");
		//_logger.LogInformation("RabbitMQ Client is trying to connect");
		lock (sync_root)
		{
			var policy = Policy.Handle<SocketException>().Or<BrokerUnreachableException>().WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(System.Math.Pow(2, retryAttempt)),
				(ex, time) => {
					Debug.WriteLine(ex.ToString());
					//_logger.LogWarning(ex.ToString());
				});
			policy.Execute(async () => { _connection = await _connectionFactory.CreateConnectionAsync(); }).Wait();
			if (IsConnected)
			{
				_connection!.ConnectionShutdownAsync += OnConnectionShutdown;
				_connection!.CallbackExceptionAsync += OnCallbackException;
				_connection!.ConnectionBlockedAsync += OnConnectionBlocked;
				Debug.WriteLine($"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");
				//_logger.LogInformation($"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");
				return Task.FromResult(true);
			}
			Debug.WriteLine("FATAL ERROR: RabbitMQ connections could not be created and opened");
			//_logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
			return Task.FromResult(false);
		}
	}
	async Task OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e)
	{
		if (_disposed) return;
		Debug.WriteLine("A RabbitMQ connection is shutdown. Trying to re-connect...");
		//_logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");
		TryConnect();
	}
	async Task OnCallbackException(object? sender, CallbackExceptionEventArgs e)
	{
		if (_disposed) return;
		Debug.WriteLine("A RabbitMQ connection throw exception. Trying to re-connect...");
		//_logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");
		TryConnect();
	}
	async Task OnConnectionShutdown(object? sender, ShutdownEventArgs reason)
	{
		if (_disposed) return;
		Debug.WriteLine("A RabbitMQ connection is on shutdown. Trying to re-connect...");
		//_logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");
		TryConnect();
	}
}