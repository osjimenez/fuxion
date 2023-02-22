namespace Fuxion.Test;

public class InternetTimeProviderTest : BaseTest<InternetTimeProviderTest>
{
	public InternetTimeProviderTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "InternetTimeProvider - NTP - CheckConsistency")]
	public void InternetTimeProvider_NTP_CheckConsistency() => new InternetTimeProvider().CheckConsistency(Output);
	[Fact(DisplayName = "InternetTimeProvider - WEB - CheckConsistency")]
	public void InternetTimeProvider_WEB_CheckConsistency() =>
		new InternetTimeProvider {
			ServerType = InternetTimeServerType.Web, ServerAddress = "https://google.com"
		}.CheckConsistency(Output);
}