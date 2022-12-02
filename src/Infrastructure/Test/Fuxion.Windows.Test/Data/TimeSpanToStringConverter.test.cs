using System.Globalization;
using Fuxion.Testing;
using Fuxion.Windows.Data;
using Fuxion.Windows.Resources;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test.Data;

public class TimeSpanToStringConverterTest : BaseTest<TimeSpanToStringConverterTest>
{
	public TimeSpanToStringConverterTest(ITestOutputHelper output) : base(output)
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("es-ES");
		Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("es-ES");
	}
	[Fact(DisplayName = "TimeSpanToStringConverter - PerElement")]
	public void TimeSpanToStringConverter_PerElements()
	{
		var con = new TimeSpanToStringConverter
		{
			Mode = TimeSpanToStringMode.PerElements
		};
		var res = con.Convert(TimeSpan.Parse("1.18:53:58.1234567"), CultureInfo.CurrentCulture);
		Assert.Contains($"1 {Strings.day}", res);
		Assert.Contains($"18 {Strings.hours}", res);
		Assert.Contains($"53 {Strings.minutes}", res);
		Assert.Contains($"58 {Strings.seconds}", res);
		Assert.Contains($"123 {Strings.milliseconds}", res);
		con.NumberOfElements = 3;
		res = con.Convert(TimeSpan.Parse("1.18:53:58.1234567"), CultureInfo.CurrentCulture);
		Assert.Contains($"1 {Strings.day}", res);
		Assert.Contains($"18 {Strings.hours}", res);
		Assert.Contains($"53 {Strings.minutes}", res);
		Assert.DoesNotContain($"58 {Strings.seconds}", res);
		Assert.DoesNotContain($"123 {Strings.milliseconds}", res);
		res = con.Convert(TimeSpan.Parse("0.18:53:58.1234567"), CultureInfo.CurrentCulture);
		Assert.DoesNotContain($"0 {Strings.day}", res);
		Assert.Contains($"18 {Strings.hours}", res);
		Assert.Contains($"53 {Strings.minutes}", res);
		Assert.Contains($"58 {Strings.seconds}", res);
		Assert.DoesNotContain($"123 {Strings.milliseconds}", res);
	}
}