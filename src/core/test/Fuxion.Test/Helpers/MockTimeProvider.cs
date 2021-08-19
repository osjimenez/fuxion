namespace Fuxion;

public class MockTimeProvider : ITimeProvider
{
	public bool MustFail { get; set; }
	public TimeSpan Offset { get; private set; }
	public void SetOffset(TimeSpan offset) => Offset = offset;
	private DateTime GetUtc()
	{
		if (MustFail) throw new MockTimeProviderException();
		return DateTime.UtcNow.Add(Offset);
	}
	public DateTime Now() => GetUtc().ToLocalTime();
	public DateTimeOffset NowOffsetted() => GetUtc().ToLocalTime();
	public DateTime UtcNow() => GetUtc();
}
public class MockTimeProviderException : FuxionException { }