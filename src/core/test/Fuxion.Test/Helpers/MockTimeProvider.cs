namespace Fuxion;

public class MockTimeProvider : ITimeProvider
{
	public bool           MustFail                   { get; set; }
	public TimeSpan       Offset                     { get; private set; }
	public DateTime       Now()                      => GetUtc().ToLocalTime();
	public DateTimeOffset NowOffsetted()             => GetUtc().ToLocalTime();
	public DateTime       UtcNow()                   => GetUtc();
	public void           SetOffset(TimeSpan offset) => Offset = offset;
	DateTime GetUtc()
	{
		if (MustFail) throw new MockTimeProviderException();
		return DateTime.UtcNow.Add(Offset);
	}
}

public class MockTimeProviderException : FuxionException { }