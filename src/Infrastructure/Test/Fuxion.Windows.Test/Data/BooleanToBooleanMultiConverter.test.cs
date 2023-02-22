using System.Globalization;
using Fuxion.Testing;
using Fuxion.Windows.Data;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test.Data;

public class BooleanToBooleanMultiConverterTest : BaseTest<BooleanToBooleanMultiConverterTest>
{
	public BooleanToBooleanMultiConverterTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "BooleanToBooleanMultiConverter - AllFalse")]
	public void BooleanToBooleanMultiConverter_AllFalse()
	{
		var c = new BooleanToBooleanMultiConverter {
			Mode = BooleanMultiConverterMode.AllFalse
		};
		Assert.False(c.Convert(new[] {
			true, true
		}, CultureInfo.CurrentCulture));
		Assert.True(c.Convert(new[] {
			false, false
		}, CultureInfo.CurrentCulture));
		Assert.False(c.Convert(new[] {
			true, false
		}, CultureInfo.CurrentCulture));
		Assert.False(c.Convert(new[] {
			true
		}, CultureInfo.CurrentCulture));
		Assert.True(c.Convert(new[] {
			false
		}, CultureInfo.CurrentCulture));
	}
	[Fact(DisplayName = "BooleanToBooleanMultiConverter - AllTrue")]
	public void BooleanToBooleanMultiConverter_AllTrue()
	{
		var c = new BooleanToBooleanMultiConverter {
			Mode = BooleanMultiConverterMode.AllTrue
		};
		Assert.True(c.Convert(new[] {
			true, true
		}, CultureInfo.CurrentCulture));
		Assert.False(c.Convert(new[] {
			false, false
		}, CultureInfo.CurrentCulture));
		Assert.False(c.Convert(new[] {
			true, false
		}, CultureInfo.CurrentCulture));
		Assert.True(c.Convert(new[] {
			true
		}, CultureInfo.CurrentCulture));
		Assert.False(c.Convert(new[] {
			false
		}, CultureInfo.CurrentCulture));
	}
	[Fact(DisplayName = "BooleanToBooleanMultiConverter - AnyFalse")]
	public void BooleanToBooleanMultiConverter_AnyFalse()
	{
		var c = new BooleanToBooleanMultiConverter {
			Mode = BooleanMultiConverterMode.AnyFalse
		};
		Assert.False(c.Convert(new[] {
			true, true
		}, CultureInfo.CurrentCulture));
		Assert.True(c.Convert(new[] {
			false, false
		}, CultureInfo.CurrentCulture));
		Assert.True(c.Convert(new[] {
			true, false
		}, CultureInfo.CurrentCulture));
		Assert.False(c.Convert(new[] {
			true
		}, CultureInfo.CurrentCulture));
		Assert.True(c.Convert(new[] {
			false
		}, CultureInfo.CurrentCulture));
	}
	[Fact(DisplayName = "BooleanToBooleanMultiConverter - AnyTrue")]
	public void BooleanToBooleanMultiConverter_AnyTrue()
	{
		var c = new BooleanToBooleanMultiConverter {
			Mode = BooleanMultiConverterMode.AnyTrue
		};
		Assert.True(c.Convert(new[] {
			true, true
		}, CultureInfo.CurrentCulture));
		Assert.False(c.Convert(new[] {
			false, false
		}, CultureInfo.CurrentCulture));
		Assert.True(c.Convert(new[] {
			true, false
		}, CultureInfo.CurrentCulture));
		Assert.True(c.Convert(new[] {
			true
		}, CultureInfo.CurrentCulture));
		Assert.False(c.Convert(new[] {
			false
		}, CultureInfo.CurrentCulture));
	}
	[Fact(DisplayName = "BooleanToBooleanMultiConverter - EmptyValue")]
	public void BooleanToBooleanMultiConverter_EmptyValue()
	{
		var c = new BooleanToBooleanMultiConverter();
		Assert.False(c.Convert(new bool[]
			{ }, CultureInfo.CurrentCulture));
	}
	[Fact(DisplayName = "BooleanToBooleanMultiConverter - NullValue")]
	public void BooleanToBooleanMultiConverter_NullValue()
	{
		var c = new BooleanToBooleanMultiConverter();
		Assert.False(c.Convert(null!, CultureInfo.CurrentCulture));
	}
}