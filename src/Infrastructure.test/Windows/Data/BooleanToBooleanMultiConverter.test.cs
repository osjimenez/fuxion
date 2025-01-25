using System.Globalization;
using Fuxion.Xunit;
using Fuxion.Windows.Data;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test.Data;

public class BooleanToBooleanMultiConverterTest(ITestOutputHelper output)
	: BaseTest<BooleanToBooleanMultiConverterTest>(output)
{
	[Fact(DisplayName = "BooleanToBooleanMultiConverter - AllFalse")]
	public void BooleanToBooleanMultiConverter_AllFalse()
	{
		var c = new BooleanToBooleanMultiConverter {
			Mode = BooleanMultiConverterMode.AllFalse
		};
		IsFalse(c.Convert([true, true], CultureInfo.CurrentCulture));
		IsTrue(c.Convert([false, false], CultureInfo.CurrentCulture));
		IsFalse(c.Convert([true, false], CultureInfo.CurrentCulture));
		IsFalse(c.Convert([true], CultureInfo.CurrentCulture));
		IsTrue(c.Convert([false], CultureInfo.CurrentCulture));
	}
	[Fact(DisplayName = "BooleanToBooleanMultiConverter - AllTrue")]
	public void BooleanToBooleanMultiConverter_AllTrue()
	{
		var c = new BooleanToBooleanMultiConverter {
			Mode = BooleanMultiConverterMode.AllTrue
		};
		IsTrue(c.Convert([true, true], CultureInfo.CurrentCulture));
		IsFalse(c.Convert([false, false], CultureInfo.CurrentCulture));
		IsFalse(c.Convert([true, false], CultureInfo.CurrentCulture));
		IsTrue(c.Convert([true], CultureInfo.CurrentCulture));
		IsFalse(c.Convert([false], CultureInfo.CurrentCulture));
	}
	[Fact(DisplayName = "BooleanToBooleanMultiConverter - AnyFalse")]
	public void BooleanToBooleanMultiConverter_AnyFalse()
	{
		var c = new BooleanToBooleanMultiConverter {
			Mode = BooleanMultiConverterMode.AnyFalse
		};
		IsFalse(c.Convert([true, true], CultureInfo.CurrentCulture));
		IsTrue(c.Convert([false, false], CultureInfo.CurrentCulture));
		IsTrue(c.Convert([true, false], CultureInfo.CurrentCulture));
		IsFalse(c.Convert([true], CultureInfo.CurrentCulture));
		IsTrue(c.Convert([false], CultureInfo.CurrentCulture));
	}
	[Fact(DisplayName = "BooleanToBooleanMultiConverter - AnyTrue")]
	public void BooleanToBooleanMultiConverter_AnyTrue()
	{
		var c = new BooleanToBooleanMultiConverter {
			Mode = BooleanMultiConverterMode.AnyTrue
		};
		IsTrue(c.Convert([true, true], CultureInfo.CurrentCulture));
		IsFalse(c.Convert([false, false], CultureInfo.CurrentCulture));
		IsTrue(c.Convert([true, false], CultureInfo.CurrentCulture));
		IsTrue(c.Convert([true], CultureInfo.CurrentCulture));
		IsFalse(c.Convert([false], CultureInfo.CurrentCulture));
	}
	[Fact(DisplayName = "BooleanToBooleanMultiConverter - EmptyValue")]
	public void BooleanToBooleanMultiConverter_EmptyValue()
	{
		var c = new BooleanToBooleanMultiConverter();
		IsFalse(c.Convert([], CultureInfo.CurrentCulture));
	}
	[Fact(DisplayName = "BooleanToBooleanMultiConverter - NullValue")]
	public void BooleanToBooleanMultiConverter_NullValue()
	{
		var c = new BooleanToBooleanMultiConverter();
		IsFalse(c.Convert(null!, CultureInfo.CurrentCulture));
	}
}