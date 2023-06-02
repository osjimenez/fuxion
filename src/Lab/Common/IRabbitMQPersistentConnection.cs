using RabbitMQ.Client;

namespace Fuxion.Lab.Common;

public interface IRabbitMQPersistentConnection : IDisposable
{
	bool IsConnected { get; }
	bool TryConnect();
	IModel CreateModel();
}