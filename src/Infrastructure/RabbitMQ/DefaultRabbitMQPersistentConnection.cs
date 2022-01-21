namespace Fuxion.RabbitMQ;

using global::RabbitMQ.Client;
using global::RabbitMQ.Client.Events;
using global::RabbitMQ.Client.Exceptions;
using Polly;
using Polly.Retry;
using System.Diagnostics;
using System.Net.Sockets;

public class DefaultRabbitMQPersistentConnection
		   : IRabbitMQPersistentConnection
{
	private readonly IConnectionFactory _connectionFactory;
	//private readonly ILogger<DefaultRabbitMQPersistentConnection> _logger;
	private readonly int _retryCount;
	private IConnection? _connection;
	private bool _disposed;
	private readonly object sync_root = new object();
	//public DefaultRabbitMQPersistentConnection(IConnectionFactory connectionFactory, ILogger<DefaultRabbitMQPersistentConnection> logger, int retryCount = 5)
	public DefaultRabbitMQPersistentConnection(IConnectionFactory connectionFactory, int retryCount = 5)
	{
		_connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
		//_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_retryCount = retryCount;
	}

	public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

	public IModel CreateModel()
	{
		if (!IsConnected)
		{
			throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
		}

		return _connection!.CreateModel();
	}

	public void Dispose()
	{
		if (_disposed) return;

		_disposed = true;

		try
		{
			_connection?.Dispose();
		}
		catch (IOException ex)
		{
			Debug.WriteLine(ex.ToString());
			//_logger.LogCritical(ex.ToString());
		}
	}

	public bool TryConnect()
	{
		Debug.WriteLine("RabbitMQ Client is trying to connect");
		//_logger.LogInformation("RabbitMQ Client is trying to connect");

		lock (sync_root)
		{
			var policy = RetryPolicy.Handle<SocketException>()
				.Or<BrokerUnreachableException>()
				.WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(System.Math.Pow(2, retryAttempt)), (ex, time) =>
				{
					Debug.WriteLine(ex.ToString());
						//_logger.LogWarning(ex.ToString());
					}
			);

			policy.Execute(() =>
			{
				_connection = _connectionFactory
					  .CreateConnection();
			});

			if (IsConnected)
			{
				_connection!.ConnectionShutdown += OnConnectionShutdown;
				_connection!.CallbackException += OnCallbackException;
				_connection!.ConnectionBlocked += OnConnectionBlocked;

				Debug.WriteLine($"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");
				//_logger.LogInformation($"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");

				return true;
			}
			else
			{
				Debug.WriteLine("FATAL ERROR: RabbitMQ connections could not be created and opened");
				//_logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");

				return false;
			}
		}
	}

	private void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e)
	{
		if (_disposed) return;

		Debug.WriteLine("A RabbitMQ connection is shutdown. Trying to re-connect...");
		//_logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

		TryConnect();
	}

	private void OnCallbackException(object? sender, CallbackExceptionEventArgs e)
	{
		if (_disposed) return;

		Debug.WriteLine("A RabbitMQ connection throw exception. Trying to re-connect...");
		//_logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

		TryConnect();
	}

	private void OnConnectionShutdown(object? sender, ShutdownEventArgs reason)
	{
		if (_disposed) return;

		Debug.WriteLine("A RabbitMQ connection is on shutdown. Trying to re-connect...");
		//_logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

		TryConnect();
	}
}