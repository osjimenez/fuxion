using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using Fuxion.Lab.Common;
using Microsoft.Extensions.Hosting;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Fuxion.Lab.Cloud.MS1;

public class RabbitHostedService : IHostedService
{
	IRabbitMQPersistentConnection _persistentConnection;
	string _queueName;
	public RabbitHostedService(string queueName, IRabbitMQPersistentConnection persistentConnection)
	{
		_queueName = queueName;
		_persistentConnection = persistentConnection;
	}
	IModel? _consumerChannel;
	IModel CreateConsumerChannel()
	{
		Console.WriteLine("============= CREATING CONSUMER CHANNEL ===============================");
		if (!_persistentConnection.IsConnected) _persistentConnection.TryConnect();
		var channel = _persistentConnection.CreateModel();
		channel.ExchangeDeclare("test-exchange", ExchangeType.Direct);
		channel.QueueDeclare(_queueName, true, false, false, null);
		var consumer = new EventingBasicConsumer(channel);
		consumer.Received += async (model, ea) => {
			//var integrationEventTypeId = ea.RoutingKey;
			var message = Encoding.UTF8.GetString(ea.Body.ToArray());
			Console.WriteLine($"Message received: {message}");
			channel.BasicAck(ea.DeliveryTag, false);
		};
		channel.BasicConsume(_queueName, false, consumer);
		channel.CallbackException += (sender, ea) => {
			_consumerChannel?.Dispose();
			_consumerChannel = CreateConsumerChannel();
		};
		return channel;
	}
	void Subscribe()
	{
		Console.WriteLine("Subscribing queue ...");
		if (!_persistentConnection.IsConnected && !_persistentConnection.TryConnect()) throw new Exception("Cannot connect to Rabbit MQ");
		using var channel = _persistentConnection.CreateModel();
		channel.QueueBind(_queueName, "test-exchange", _queueName,new Dictionary<string, object>());
	}
	public Task StartAsync(CancellationToken cancellationToken)
	{
		Console.WriteLine("Starting RabbitHostedService");
		_consumerChannel = CreateConsumerChannel();
		Subscribe();
		return Task.CompletedTask;
	} 
	public Task StopAsync(CancellationToken cancellationToken)
	{
		Console.WriteLine("Stopping RabbitHostedService");
		return Task.CompletedTask;
	}

	public static async Task Send(IRabbitMQPersistentConnection persistentConnection, string rountingKey, string message)
	{
		if (!persistentConnection.IsConnected) persistentConnection.TryConnect();
		var policy = Policy.Handle<BrokerUnreachableException>().Or<SocketException>().WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(System.Math.Pow(2, retryAttempt)),
			(ex, time) => {
				Console.WriteLine(ex.ToString());
			});
		using var channel = persistentConnection.CreateModel();
		channel.ExchangeDeclare("test-exchange", ExchangeType.Direct);
		var body = Encoding.UTF8.GetBytes(message);
		await policy.ExecuteAsync(() => {
			var properties = channel.CreateBasicProperties();
			properties.DeliveryMode = 2; // persistent
			channel.BasicPublish("test-exchange", rountingKey, true, properties, body);
			return Task.CompletedTask;
		});
	}
}