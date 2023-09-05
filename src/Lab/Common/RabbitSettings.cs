namespace Fuxion.Lab.Common;

public class RabbitSettings
{
	public string Host { get; set; } = "";
	public int Port { get; set; }
	public string Exchange { get; set; } = "";
	public string QueuePrefix { get; set; } = "";
}