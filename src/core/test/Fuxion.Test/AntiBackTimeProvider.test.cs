namespace Fuxion.Test;

public class AntiBackTimeProviderTest : BaseTest<AntiBackTimeProviderTest>
{
	public AntiBackTimeProviderTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "AntiBackTimeProvider - BackTimeException")]
	public void AntiBackTimeProvider_BackTimeException()
	{
		var mock = new MockTimeProvider();
		var abtp = new AntiBackTimeProvider(new MemoryStoredTimeProvider().Transform(s =>
		{
			s.SaveUtcTime(DateTime.UtcNow);
			return s;
		}))
		{
			TimeProvider = mock, Logger = Logger
		};
		mock.SetOffset(TimeSpan.FromDays(-1));
		Assert.Throws<BackTimeException>(() => abtp.UtcNow());
	}
	[Fact(DisplayName = "AntiBackTimeProvider - CheckConsistency")]
	public void AntiBackTimeProvider_CheckConsistency() =>
		new AntiBackTimeProvider(new MockStorageTimeProvider().Transform(s =>
		{
			s.SaveUtcTime(DateTime.UtcNow);
			return s;
		})).CheckConsistency(Output);
}

public class MockStorageTimeProvider : StoredTimeProvider
{
	DateTime value;
	public override DateTime GetUtcTime() => value;
	public override void SaveUtcTime(DateTime time) => value = time.ToUniversalTime();
}