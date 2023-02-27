using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Licensing.Test;

public static class ITimeProviderExtensions
{
	public static void CheckConsistency(this ITimeProvider provider, ITestOutputHelper? output = null)
	{
		output?.WriteLine("".PadRight(60, '='));
		output?.WriteLine("");
		output?.WriteLine($"   Checking consistency for '{provider.GetType().Name}'");
		output?.WriteLine("");
		var pad = 30;
		var now = provider.Now();
		var nowOff = provider.NowOffsetted();
		var utcNow = provider.UtcNow();
		output?.WriteLine($"{now.ToString("HH:mm:ss.fff").PadRight(pad)} provider.Now()");
		output?.WriteLine($"{nowOff.DateTime.ToString("HH:mm:ss.fff zzz").PadRight(pad)} provider.NowOffsetted()");
		output?.WriteLine($"{utcNow.ToString("HH:mm:ss.fff").PadRight(pad)} provider.UtcNow()");
		Assert.True(System.Math.Abs(now.Subtract(nowOff.Offset).Subtract(utcNow).TotalSeconds) <= 30, "now - nowOff.Offset must be UTC time (with 30 seconds of margin)");
		Assert.True(now.Subtract(nowOff.DateTime).TotalSeconds <= 30, "now must be same as nowOff.DateTime (with 30 seconds of margin)");
		output?.WriteLine("");
		output?.WriteLine("   PASSED");
		output?.WriteLine("");
		output?.WriteLine("".PadRight(60, '='));
	}
}