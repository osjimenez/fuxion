namespace Fuxion.Windows.Test.Data;

using Fuxion.Testing;
using Fuxion.Windows.Data;
using System.Globalization;
using System.Windows;
using Xunit;
using Xunit.Abstractions;

public class PipeConverterTest : BaseTest
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