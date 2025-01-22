using RabbitMQ.Client;

namespace Fuxion.Lab.Common;

public interface IRabbitMQPersistentConnection : IDisposable
{
	bool IsConnected { get; }
	Task<bool> TryConnect();
	Task<IChannel> CreateModel();
}