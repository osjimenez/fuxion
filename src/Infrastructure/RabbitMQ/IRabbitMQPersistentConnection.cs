using RabbitMQ.Client;

namespace Fuxion.RabbitMQ;

public interface IRabbitMQPersistentConnection : IDisposable
{
	bool IsConnected { get; }
	Task<bool> TryConnect();
	Task<IChannel> CreateModel(CancellationToken ct = default);
}