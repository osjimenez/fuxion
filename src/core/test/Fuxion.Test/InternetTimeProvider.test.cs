namespace Fuxion.Test;

public class InternetTimeProviderTest
{
	public InternetTimeProviderTest(ITestOutputHelper output) => this.output = output;
	readonly ITestOutputHelper output;
	[Fact(DisplayName = "InternetTimeProvider - NTP - CheckConsistency")]
	public void InternetTimeProvider_NTP_CheckConsistency() => new InternetTimeProvider().CheckConsistency(output);
	[Fact(DisplayName = "InternetTimeProvider - WEB - CheckConsistency")]
	public void InternetTimeProvider_WEB_CheckConsistency() =>
		new InternetTimeProvider
		{
			ServerType = InternetTimeServerType.Web,
			ServerAddress = "https://google.com"
		}.CheckConsistency(output);
}