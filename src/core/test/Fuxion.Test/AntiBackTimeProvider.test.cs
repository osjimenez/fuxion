namespace Fuxion.Test;

using Fuxion.Testing;

public class AntiBackTimeProviderTest : BaseTest
{
	public AntiBackTimeProviderTest(ITestOutputHelper output) : base(output) => this.output = output;

	private readonly ITestOutputHelper output;
	[Fact(DisplayName = "AntiBackTimeProvider - CheckConsistency")]
	public void AntiBackTimeProvider_CheckConsistency() => new AntiBackTimeProvider(new MockStorageTimeProvider().Transform(s =>
		{
			s.SaveUtcTime(DateTime.UtcNow);
			return s;
		}))
		.CheckConsistency(output);

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
			TimeProvider = mock,
			Logger = new XunitLogger(output)
		};
		mock.SetOffset(TimeSpan.FromDays(-1));
		Assert.Throws<BackTimeException>(() => abtp.UtcNow());
	}
}
public class MockStorageTimeProvider : StoredTimeProvider
{
	private DateTime value;
	public override DateTime GetUtcTime() => value;
	public override void SaveUtcTime(DateTime time) => value = time.ToUniversalTime();
}