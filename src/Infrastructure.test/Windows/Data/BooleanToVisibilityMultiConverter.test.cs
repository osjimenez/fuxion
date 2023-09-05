using System.Globalization;
using System.Windows;
using Fuxion.Xunit;
using Fuxion.Windows.Data;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test.Data;

public class BooleanToVisibilityMultiConverterTest : BaseTest<BooleanToVisibilityMultiConverterTest>
{
	public BooleanToVisibilityMultiConverterTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "BooleanToVisibilityMultiConverter - AllFalse")]
	public void BooleanToVisibilityMultiConverter_AllFalse()
	{
		var c = new BooleanToVisibilityMultiConverter {
			Mode = BooleanMultiConverterMode.AllFalse
		};
		var res = c.Convert(new[] {
			true, true
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Collapsed, res);
		res = c.Convert(new[] {
			false, false
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Visible, res);
		res = c.Convert(new[] {
			true, false
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Collapsed, res);
		res = c.Convert(new[] {
			true
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Collapsed, res);
		res = c.Convert(new[] {
			false
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Visible, res);
	}
	[Fact(DisplayName = "BooleanToVisibilityMultiConverter - AllTrue")]
	public void BooleanToVisibilityMultiConverter_AllTrue()
	{
		var c = new BooleanToVisibilityMultiConverter {
			Mode = BooleanMultiConverterMode.AllTrue
		};
		var res = c.Convert(new[] {
			true, true
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Visible, res);
		res = c.Convert(new[] {
			false, false
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Collapsed, res);
		res = c.Convert(new[] {
			true, false
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Collapsed, res);
		res = c.Convert(new[] {
			true
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Visible, res);
		res = c.Convert(new[] {
			false
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Collapsed, res);
	}
	[Fact(DisplayName = "BooleanToVisibilityMultiConverter - AnyFalse")]
	public void BooleanToVisibilityMultiConverter_AnyFalse()
	{
		var c = new BooleanToVisibilityMultiConverter {
			Mode = BooleanMultiConverterMode.AnyFalse
		};
		var res = c.Convert(new[] {
			true, true
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Collapsed, res);
		res = c.Convert(new[] {
			false, false
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Visible, res);
		res = c.Convert(new[] {
			true, false
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Visible, res);
		res = c.Convert(new[] {
			true
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Collapsed, res);
		res = c.Convert(new[] {
			false
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Visible, res);
	}
	[Fact(DisplayName = "BooleanToVisibilityMultiConverter - AnyTrue")]
	public void BooleanToVisibilityMultiConverter_AnyTrue()
	{
		var c = new BooleanToVisibilityMultiConverter {
			Mode = BooleanMultiConverterMode.AnyTrue
		};
		var res = c.Convert(new[] {
			true, true
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Visible, res);
		res = c.Convert(new[] {
			false, false
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Collapsed, res);
		res = c.Convert(new[] {
			true, false
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Visible, res);
		res = c.Convert(new[] {
			true
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Visible, res);
		res = c.Convert(new[] {
			false
		}, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Collapsed, res);
	}
	[Fact(DisplayName = "BooleanToVisibilityMultiConverter - EmptyValue")]
	public void BooleanToVisibilityMultiConverter_EmptyValue()
	{
		var c = new BooleanToVisibilityMultiConverter {
			EmptyValue = Visibility.Hidden
		};
		var res = c.Convert(new bool[]
			{ }, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Hidden, res);
	}
	[Fact(DisplayName = "BooleanToVisibilityMultiConverter - NullValue")]
	public void BooleanToVisibilityMultiConverter_NullValue()
	{
		var c = new BooleanToVisibilityMultiConverter {
			NullValue = Visibility.Hidden
		};
		var res = c.Convert(null!, CultureInfo.CurrentCulture);
		Assert.Equal(Visibility.Hidden, res);
	}
}