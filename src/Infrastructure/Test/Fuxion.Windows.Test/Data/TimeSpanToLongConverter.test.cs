namespace Fuxion.Windows.Test.Data;

using Fuxion.Testing;
using Fuxion.Windows.Data;
using Xunit;
using Xunit.Abstractions;

public class ConverterTest : BaseTest
{
	public ConverterTest(ITestOutputHelper output) : base(output) { }
	protected void Convert() { }
}
public class TimeSpanToLongConverterTest : BaseTest
{
	public TimeSpanToLongConverterTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "TimeSpanToLongConverter - ConvertToSeconds")]
	public void ConvertToSeconds()
	{
		var conv = new TimeSpanToLongConverter
		{
			Value = TimeSpanToLongValue.Seconds
		};

		//int res = (int)((IValueConverter)conv).Convert(TimeSpan.FromTicks(700000000), typeof(TimeSpan?), null, CultureInfo.CurrentCulture);
		//var res = conv.Convert(TimeSpan.FromTicks(700000000), CultureInfo.CurrentCulture);
		//Assert.Equal(10, res);

		//res = conv.Convert(TimeSpan.FromMilliseconds(70000), CultureInfo.CurrentCulture);
		//Assert.Equal(10, res);

		//res = conv.Convert(TimeSpan.FromSeconds(70), CultureInfo.CurrentCulture);
		//Assert.Equal(10, res);

		//res = conv.Convert(TimeSpan.FromMinutes(1.18), CultureInfo.CurrentCulture);
		//Assert.Equal(10, res);

		//res = conv.Convert(TimeSpan.FromHours(0.0028), CultureInfo.CurrentCulture);
		//Assert.Equal(10, res);

		//res = conv.Convert(TimeSpan.FromDays(0.00012), CultureInfo.CurrentCulture);
		//Assert.Equal(10, res);
	}
}