namespace Fuxion;

[DefaultSingletonInstance(typeof(LocalMachineTimeProvider))]
public interface ITimeProvider
{
	DateTime Now();
	DateTimeOffset NowOffsetted();
	DateTime UtcNow();
}