using Fuxion.Test;
using Fuxion.Windows.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test.Data
{
	public class StringToCapitalizedStringConverterTest : BaseTest
	{
		public StringToCapitalizedStringConverterTest(ITestOutputHelper output) : base(output) { }
		[Fact(DisplayName = "StringToCapitalizedStringConverter - ToUpper")]
		public void StringToCapitalizedStringConverter_ToUpper()
		{
			var res = new StringToCapitalizedStringConverter { Capitalization = StringCapitalization.ToUpper}.Convert("test String", CultureInfo.CurrentCulture);
			Assert.Equal("TEST STRING", res);
		}
		[Fact(DisplayName = "StringToCapitalizedStringConverter - ToLower")]
		public void StringToCapitalizedStringConverter_ToLower()
		{
			var res = new StringToCapitalizedStringConverter { Capitalization = StringCapitalization.ToLower }.Convert("test String", CultureInfo.CurrentCulture);
			Assert.Equal("test string", res);
		}
		[Fact(DisplayName = "StringToCapitalizedStringConverter - ToTitleCase")]
		public void StringToCapitalizedStringConverter_ToTitleCase()
		{
			var res = new StringToCapitalizedStringConverter { Capitalization = StringCapitalization.ToTitleCase }.Convert("test String", CultureInfo.CurrentCulture);
			Assert.Equal("Test String", res);
		}
		[Fact(DisplayName = "StringToCapitalizedStringConverter - ToCamelCase")]
		public void StringToCapitalizedStringConverter_ToCamelCase()
		{
			var res = new StringToCapitalizedStringConverter { Capitalization = StringCapitalization.ToCamelCase }.Convert("test String", CultureInfo.CurrentCulture);
			Assert.Equal("testString", res);
		}
		[Fact(DisplayName = "StringToCapitalizedStringConverter - ToPascalCase")]
		public void StringToCapitalizedStringConverter_ToPascalCase()
		{
			var res = new StringToCapitalizedStringConverter { Capitalization = StringCapitalization.ToPascalCase }.Convert("test String", CultureInfo.CurrentCulture);
			Assert.Equal("TestString", res);
		}
	}
}
