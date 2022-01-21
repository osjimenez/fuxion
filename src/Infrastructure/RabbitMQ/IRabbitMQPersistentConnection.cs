namespace Fuxion.RabbitMQ;

using global::RabbitMQ.Client;

public interface IRabbitMQPersistentConnection
		: IDisposable
{
	bool IsConnected { get; }

	bool TryConnect();

	IModel CreateModel();
}