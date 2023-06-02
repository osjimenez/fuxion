using Fuxion.Reflection;

namespace Fuxion.Lab.Common;

[TypeKey("https://fuxion.dev", "routing", "RabbitMQ", "send-route-info")]
public class RabbitMQSend
{
	public ReadOnlyMemory<byte> Body { get; set; }
	public string RoutingKey { get; set; } = "";
}
[TypeKey("https://fuxion.dev", "routing", "RabbitMQ", "receive-route-info")]
public class RabbitMQReceive
{
	public ReadOnlyMemory<byte> Body { get; set; }
	public string Source { get; set; } = "";
}