namespace Fuxion.Windows.Test.Data;

using Fuxion.Testing;
using Fuxion.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Xunit;
using Xunit.Abstractions;

public class GenericConverterTest : BaseTest
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