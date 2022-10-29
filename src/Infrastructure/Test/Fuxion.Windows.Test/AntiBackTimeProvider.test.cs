using Fuxion.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test;

public class RegistryStoredTimeProviderTest : BaseTest
{
	public RegistryStoredTimeProviderTest(ITestOutputHelper output) : base(output) => this.output = output;
	readonly ITestOutputHelper output;
	[Fact(DisplayName = "RegistryStoredTimeProvider - CheckConsistency")]
	public void RegistryStorageTimeProvider_CheckConsistency() =>
		new AntiBackTimeProvider(new RegistryStoredTimeProvider().Transform(s =>
		{
			s.SaveUtcTime(DateTime.UtcNow);
			return s;
		})).CheckConsistency(output);
}

public class MockStorageTimeProvider : StoredTimeProvider
{
	DateTime                 value;
	public override DateTime GetUtcTime()               => value;
	public override void     SaveUtcTime(DateTime time) => value = time.ToUniversalTime();
}