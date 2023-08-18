using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Fuxion.Xunit;
using Fuxion.Windows.Data;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test.Data;

public class IntegerToVisibilityConverterTest : BaseTest<IntegerToVisibilityConverterTest>
{
	public IntegerToVisibilityConverterTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "IntegerToVisibilityConverter - Convert")]
	public void IntegerToVisibilityConverter_Convert()
	{
		var converter = new IntegerToVisibilityConverter {
			VisibleValuesCommaSeparated = "1 ,2,3", CollapsedValuesCommaSeparated = "3, 4,5", HiddenValuesCommaSeparated = "5, 6 ,7"
		};
		var res = ((IValueConverter)converter).Convert(1, typeof(EnumTest?), null, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Visible, res);
		res = ((IValueConverter)converter).Convert(2, typeof(EnumTest?), null, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Visible, res);
		res = ((IValueConverter)converter).Convert(3, typeof(EnumTest?), null, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Visible, res);
		res = ((IValueConverter)converter).Convert(4, typeof(EnumTest?), null, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Collapsed, res);
		res = ((IValueConverter)converter).Convert(5, typeof(EnumTest?), null, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Collapsed, res);
		res = ((IValueConverter)converter).Convert(6, typeof(EnumTest?), null, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Hidden, res);
		res = ((IValueConverter)converter).Convert(7, typeof(EnumTest?), null, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Hidden, res);
		res = ((IValueConverter)converter).Convert(8, typeof(EnumTest?), null, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Collapsed, res);
	}
}