using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Fuxion.Xunit;
using Fuxion.Windows.Data;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test.Data;

public class NullToVisibilityConverterTest : BaseTest<NullToVisibilityConverterTest>
{
	public NullToVisibilityConverterTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "NullToVisibilityConverter - Enum value")]
	public void NullToVisibilityConverter_DisplayValue()
	{
		var res = ((IValueConverter)new NullToVisibilityConverter()).Convert(null, typeof(EnumTest?), null, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Collapsed, res);
	}
}