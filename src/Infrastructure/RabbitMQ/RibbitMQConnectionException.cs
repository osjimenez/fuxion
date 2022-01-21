namespace Fuxion.RabbitMQ;

public class RabbitMQConnectionException : FuxionException
{
	public RabbitMQConnectionException(string message) : base(message) { }
}