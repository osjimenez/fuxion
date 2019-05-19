using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test
{
	public class LocalMachineTimeProviderTest
	{
		public LocalMachineTimeProviderTest(ITestOutputHelper output) => this.output = output;

		private readonly ITestOutputHelper output;
		[Fact(DisplayName = "LocalMachineTimeProvider - CheckConsistency")]
		public void LocalMachineTimeProvider_CheckConsistency() => new LocalMachinneTimeProvider().CheckConsistency(output);
	}
}
