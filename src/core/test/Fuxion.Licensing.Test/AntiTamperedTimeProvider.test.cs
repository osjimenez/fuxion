namespace Fuxion.Licensing.Test;

using Fuxion.Testing;
using Xunit;
using Xunit.Abstractions;

public class AntiTamperedTimeProviderTest : BaseTest
{
	public AntiTamperedTimeProviderTest(ITestOutputHelper output) : base(output) { }


	private string[] WebServersAddresses { get; } = new[]
{
			"http://www.google.com",
			"http://www.google.es",
            //"http://www.ooooooo-youtube.com",
            "http://www.microsoft.com",
			"http://www.yahoo.com",
			"http://www.amazon.com",
			"http://www.facebook.com",
			"http://www.twitter.com",
		};
	[Fact(DisplayName = "AntiTamperedTimeProvider - CheckConsistency")]
	public void AntiTamperedTimeProvider_CheckConsistency()
	{
		var atp = new AverageTimeProvider
		{
			Logger = new XunitLogger(Output),
			MaxFailsPerTry = 1,
			RandomizedProvidersPerTry = WebServersAddresses.Length
		};
		foreach (var pro in WebServersAddresses.Select(address => new InternetTimeProvider
		{
			ServerAddress = address,
			ServerType = InternetTimeServerType.Web,
			Timeout = TimeSpan.FromSeconds(5)
		})) atp.AddProvider(pro, true);
		new AntiTamperedTimeProvider(atp, new AntiBackTimeProvider(new MemoryStoredTimeProvider().Transform(s =>
		{
			s.SaveUtcTime(DateTime.UtcNow);
			return s;
		}))
		{
			Logger = new XunitLogger(Output)
		})
			.CheckConsistency(Output);
	}
}