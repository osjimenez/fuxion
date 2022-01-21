﻿namespace Fuxion.Windows.Test.Data;

using Fuxion.Testing;
using Fuxion.Windows.Data;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows.Data;
using Xunit;
using Xunit.Abstractions;

public class EnumToNamedEnumValueConverterTest : BaseTest
{
	public EnumToNamedEnumValueConverterTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "EnumToNamedEnumValueConverter - Display value")]
	public void EnumToNamedEnumValueConverter_DisplayValue()
	{
		var res = new EnumToNamedEnumValueConverter().Convert(EnumTest.One, CultureInfo.CurrentCulture);
		Assert.Equal("One value", res.ToString());
	}
	[Fact(DisplayName = "EnumToNamedEnumValueConverter - Null value")]
	public void EnumToNamedEnumValueConverter_NullValue()
	{
		Assert.Throws<NotSupportedException>(() =>
		{
			((IValueConverter)new EnumToNamedEnumValueConverter()).Convert(null, typeof(EnumTest), null, CultureInfo.CurrentCulture);
		});
		var res = ((IValueConverter)new NullableEnumToNamedEnumValueConverter()).Convert(null, typeof(EnumTest), null, CultureInfo.CurrentCulture);
		Assert.Null(res?.ToString());
	}
}
public enum EnumTest
{
	[Display(Name = "One value")]
	One,
	[Display(Name = "Two value")]
	Two
}