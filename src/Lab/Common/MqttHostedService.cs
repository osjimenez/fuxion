using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Packets;
using MQTTnet.Protocol;

namespace Fuxion.Lab.Common;

public class MqttHostedService : IHostedService
{
	protected CancellationToken ServiceCancellationToken;
	ILogger<MqttHostedService> _logger;
	string _host;
	int _port;
	string _topic;
	public MqttHostedService(string host, int port, string topic, ILogger<MqttHostedService> logger)
	{
		_host = host;
		_port = port;
		_topic = topic;
		_logger = logger;
	}
	static IMqttClient? MqttClient { get; set; }
	
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		ServiceCancellationToken = cancellationToken;
		_logger.LogInformation("Creating MQTT client...");
		MqttFactory factory = new();

		MqttClient = factory.CreateMqttClient();

		// Register a handler when MQTT client disconnects to the broker
		MqttClient.DisconnectedAsync += async args =>
		{
			_logger.LogInformation("MqttClient disconnected from server");
			_logger.LogInformation("Reason: {ArgsReason}", args.Reason);
			_logger.LogInformation("ClientWasConnected: {ArgsClientWasConnected}", args.ClientWasConnected);

			await Task.Delay(TimeSpan.FromSeconds(5), ServiceCancellationToken);
			await ConnectAsync(ServiceCancellationToken);
		};
		// Register a handler when MQTT client connects to the broker
		MqttClient.ConnectedAsync += async arg =>
		{
			// $"devices/{MqttConfig.Id}/commands/#"
			MqttClientSubscribeResult? result = await MqttClient.SubscribeAsync(_topic);
			if (result.Items.First().ResultCode == MqttClientSubscribeResultCode.NotAuthorized)
			{
				_logger.LogWarning("Command Subscribe Failed (NOT AUTHORIZED)");
			}
			else
			{
				_logger.LogInformation("Command subscribe result: {ResultCode}", result.Items.First().ResultCode);
			}
			//await SubscribeToDefaultTopics();
		};
		// Register a handler when a message is received
		MqttClient.ApplicationMessageReceivedAsync += args =>
		{
			var message = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
			Console.WriteLine($"Mensaje MQTT recibido: " + message);
			return Task.CompletedTask;
		};


		// Connect to an MQTT broker
		await ConnectAsync(ServiceCancellationToken);

		_logger.LogInformation("MQTT client has been created successfully");
	}
	public async Task StopAsync(CancellationToken cancellationToken) => await MqttClient.DisconnectAsync();
	private async Task ConnectAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Connecting MQTT ...");
		try
		{
			MqttClientConnectResult? result = await MqttClient!.ConnectAsync(new MqttClientOptionsBuilder()
																								  //.WithClientId(MqttConfig.Id ?? Guid.NewGuid().ToString())
																								  .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
																								  .WithProtocolVersion(MqttProtocolVersion.V500)
																								  //.WithCleanSession()
																								  //.WithCredentials("admin", "admin")
																								  .WithTcpServer(_host, _port).Build(), cancellationToken);

			if (result.ResultCode == MqttClientConnectResultCode.Success)
			{
				_logger.LogDebug("ResultCode: {ResultResultCode}", result.ResultCode);
				return;
			}

			_logger.LogError("ResultCode: {ResultResultCode}", result.ResultCode);
			_logger.LogError("Reason: {ResultReasonString}", result.ReasonString);
			_logger.LogError("ResponseInformation: {ResultResponseInformation}", result.ResponseInformation);
		}
		catch (Exception e)
		{
			_logger.LogError($"MqttClient connection Failed.\r\nException: {e.GetType().Name}\r\nMessage: {e.Message}\r\n{e}");
			await Task.Delay(5000, cancellationToken);
		}
	}
	public static async Task<MqttClientPublishResult> Publish(string topic, string message)
	{
		MqttApplicationMessage? appMessage = new MqttApplicationMessageBuilder()
														 .WithTopic(topic)
														 .WithPayload(message)
														 .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
														 .Build();

		if (MqttClient is null)
		{
			Console.WriteLine("MqttClient not defined");
			return new(null, MqttClientPublishReasonCode.ImplementationSpecificError, "",ReadOnlyCollection<MqttUserProperty>.Empty);
			// return new() {ReasonCode = MqttClientPublishReasonCode.ImplementationSpecificError};
		}

		if (!MqttClient.IsConnected)
		{
			Console.WriteLine("MqttClient is not connected");
			return new(null, MqttClientPublishReasonCode.ImplementationSpecificError, "",ReadOnlyCollection<MqttUserProperty>.Empty);
			// return new() {ReasonCode = MqttClientPublishReasonCode.ImplementationSpecificError};
		}

		MqttClientPublishResult result = await MqttClient.PublishAsync(appMessage);

		if (!result.IsSuccess)
		{
			Console.WriteLine("Publish result is not success: {ResultReasonCode}", result.ReasonCode);
		}

		return result;
	}
}