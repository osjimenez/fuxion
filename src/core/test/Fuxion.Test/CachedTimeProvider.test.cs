namespace Fuxion.Test;

public class CachedTimeProviderTest : BaseTest<CachedTimeProviderTest>
{
	public CachedTimeProviderTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "CachedTimeProvider - CacheTest")]
	public void CachedTimeProvider_CacheTest()
	{
		var ctp = new CachedTimeProvider(new LocalMachinneTimeProvider()) {
			Logger = Logger, ExpirationInterval = TimeSpan.FromSeconds(1)
		};
		ctp.UtcNow(out var fromCache);
		Assert.False(fromCache);
		ctp.UtcNow(out fromCache);
		Assert.True(fromCache);
		Thread.Sleep(1000);
		ctp.UtcNow(out fromCache);
		Assert.False(fromCache);
		ctp.UtcNow(out fromCache);
		Assert.True(fromCache);
	}
	[Fact(DisplayName = "CachedTimeProvider - CheckConsistency")]
	public void CachedTimeProvider_CheckConsistency() => new CachedTimeProvider(new LocalMachinneTimeProvider()).CheckConsistency(Output);
}