using Fuxion.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test;

public class RegistryStoredTimeProviderTest : BaseTest<RegistryStoredTimeProviderTest>
{
	public RegistryStoredTimeProviderTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "RegistryStoredTimeProvider - CheckConsistency")]
	public void RegistryStorageTimeProvider_CheckConsistency() =>
		new AntiBackTimeProvider(new RegistryStoredTimeProvider().Transform(s => {
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