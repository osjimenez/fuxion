using Fuxion.Pods;
using Fuxion.Reflection;

namespace Fuxion.Lab.Common;

[UriKey(UriKey.FuxionBaseUri + "lab/common/rabbitmqsend")]
public class RabbitMQSend
{
	public ReadOnlyMemory<byte> Body { get; set; }
	public string RoutingKey { get; set; } = "";
}
[UriKey(UriKey.FuxionBaseUri + "lab/common/rabbitmqreceive")]
public class RabbitMQReceive
{
	public ReadOnlyMemory<byte> Body { get; set; }
	public string Source { get; set; } = "";
}