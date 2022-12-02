using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Fuxion.Testing;
using Fuxion.Windows.Data;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test.Data;

public class GenericConverterTest : BaseTest<GenericConverterTest>
{
	public GenericConverterTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "GenericConverterTest - UnsetValues")]
	public void GenericMultiConverterTest_UnsetValues()
	{
		var c = new BooleanToVisibilityConverter
		{
			TrueValue = Visibility.Hidden
		};
		var res = new Func<Visibility>(() => (Visibility)((IValueConverter)c).Convert(DependencyProperty.UnsetValue, typeof(bool), null, CultureInfo.CurrentCulture));
		Assert.Throws<NotSupportedException>(() => res());
		c.AllowUnsetValue = true;
		Assert.Equal(Visibility.Visible, res());
		c.UnsetValue = Visibility.Hidden;
		Assert.Equal(Visibility.Hidden, res());
	}
}