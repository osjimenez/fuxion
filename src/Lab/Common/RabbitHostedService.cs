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
	IChannel? _consumerChannel;
	async Task<IChannel> CreateConsumerChannel()
	{
		Console.WriteLine("============= CREATING CONSUMER CHANNEL ===============================");
		if (!_persistentConnection.IsConnected) _persistentConnection.TryConnect();
		var channel = await _persistentConnection.CreateModel();
		await channel.ExchangeDeclareAsync("test-exchange", ExchangeType.Direct);
		await channel.QueueDeclareAsync(_queueName, true, false, false, null);
		var consumer = new AsyncEventingBasicConsumer(channel);
		consumer.ReceivedAsync += async (model, ea) => {
			//var integrationEventTypeId = ea.RoutingKey;
			var message = Encoding.UTF8.GetString(ea.Body.ToArray());
			Console.WriteLine($"Message received: {message}");
			await channel.BasicAckAsync(ea.DeliveryTag, false);
		};
		await channel.BasicConsumeAsync(_queueName, false, consumer);
		channel.CallbackExceptionAsync += async (sender, ea) => {
			_consumerChannel?.Dispose();
			_consumerChannel = await CreateConsumerChannel();
		};
		return channel;
	}
	async Task Subscribe()
	{
		Console.WriteLine("Subscribing queue ...");
		if (!_persistentConnection.IsConnected && await _persistentConnection.TryConnect()) throw new Exception("Cannot connect to Rabbit MQ");
		await using var channel = await _persistentConnection.CreateModel();
		await channel.QueueBindAsync(_queueName, "test-exchange", _queueName,new Dictionary<string, object?>());
	}
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		Console.WriteLine("Starting RabbitHostedService");
		_consumerChannel = await CreateConsumerChannel();
		await Subscribe();
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
		await using var channel = await persistentConnection.CreateModel();
		await channel.ExchangeDeclareAsync("test-exchange", ExchangeType.Direct);
		var body = Encoding.UTF8.GetBytes(message);
		await policy.ExecuteAsync(async () => {
			var properties = new BasicProperties();
			properties.DeliveryMode = DeliveryModes.Persistent; // persistent
			await channel.BasicPublishAsync("test-exchange", rountingKey, true, properties, body);
			return Task.CompletedTask;
		});
	}
}