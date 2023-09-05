using System.Globalization;
using System.Windows;
using Fuxion.Xunit;
using Fuxion.Windows.Data;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test.Data;

public class PipeConverterTest : BaseTest<PipeConverterTest>
{
	public PipeConverterTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "PipeConverter - First")]
	public void First()
	{
		var con = new PipeConverter();
		con.Converters.Add(new BooleanToNegateBooleanConverter());
		con.Converters.Add(new BooleanToVisibilityConverter());
		var res = con.Convert(false, typeof(bool), null, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Visible, res);
	}
}