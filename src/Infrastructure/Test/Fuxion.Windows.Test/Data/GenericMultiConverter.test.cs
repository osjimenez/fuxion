using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Fuxion.Testing;
using Fuxion.Windows.Data;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test.Data;

public class GenericMultiConverterTest : BaseTest<GenericMultiConverterTest>
{
	public GenericMultiConverterTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "GenericMultiConverterTest - UnsetValues")]
	public void GenericMultiConverterTest_UnsetValues()
	{
		var c = new BooleanToVisibilityMultiConverter
		{
			TrueValue = Visibility.Hidden
		};
		var res = new Func<Visibility>(() => (Visibility)((IMultiValueConverter)c).Convert(new[]
		{
			true, DependencyProperty.UnsetValue
		}, typeof(bool), null, CultureInfo.CurrentCulture));
		Assert.Throws<NotSupportedException>(() => res());
		c.AllowUnsetValues = true;
		Assert.Equal(Visibility.Hidden, res());
		c.IgnoreUnsetValues = false;
		Assert.NotEqual(Visibility.Hidden, res());
		c.UnsetValue = Visibility.Hidden;
		Assert.Equal(Visibility.Hidden, res());
	}
}