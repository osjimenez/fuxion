namespace Fuxion.Test;

public class InternetTimeProviderTest
{
	public InternetTimeProviderTest(ITestOutputHelper output) => this.output = output;

	private readonly ITestOutputHelper output;
	[Fact(DisplayName = "InternetTimeProvider - CheckConsistency")]
	public void InternetTimeProvider_CheckConsistency() => new InternetTimeProvider().CheckConsistency(output);
}