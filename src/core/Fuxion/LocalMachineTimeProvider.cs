namespace Fuxion;

public class LocalMachineTimeProvider : ITimeProvider
{
	public DateTime Now() => DateTime.Now;
	public DateTimeOffset NowOffsetted() => DateTimeOffset.Now;
	public DateTime UtcNow() => DateTime.UtcNow;
}