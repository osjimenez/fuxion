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
		string[] testCases = new[]
		{
			"test string",
			"test String",
			"test StRing",
			"test stRing",
			"TEST STRING",
		};
		[Fact(DisplayName = "StringToCapitalizedStringConverter - ToUpper")]
		public void StringToCapitalizedStringConverter_ToUpper()
		{
			foreach (var @case in testCases)
			{
				var res = new StringToCapitalizedStringConverter { Capitalization = StringCapitalization.ToUpper }.Convert(@case, CultureInfo.CurrentCulture);
				Assert.Equal("TEST STRING", res);
			}
		}
		[Fact(DisplayName = "StringToCapitalizedStringConverter - ToLower")]
		public void StringToCapitalizedStringConverter_ToLower()
		{
			foreach (var @case in testCases)
			{
				var res = new StringToCapitalizedStringConverter { Capitalization = StringCapitalization.ToLower }.Convert(@case, CultureInfo.CurrentCulture);
				Assert.Equal("test string", res);
			}
		}
		[Fact(DisplayName = "StringToCapitalizedStringConverter - ToTitleCase")]
		public void StringToCapitalizedStringConverter_ToTitleCase()
		{
			foreach (var @case in testCases)
			{
				var res = new StringToCapitalizedStringConverter { Capitalization = StringCapitalization.ToTitleCase }.Convert(@case, CultureInfo.CurrentCulture);
				Assert.Equal("Test String", res);
			}
		}
		[Fact(DisplayName = "StringToCapitalizedStringConverter - ToCamelCase")]
		public void StringToCapitalizedStringConverter_ToCamelCase()
		{
			foreach (var @case in testCases)
			{
				var res = new StringToCapitalizedStringConverter { Capitalization = StringCapitalization.ToCamelCase }.Convert(@case, CultureInfo.CurrentCulture);
				Assert.Equal("testString", res);
			}
		}
		[Fact(DisplayName = "StringToCapitalizedStringConverter - ToPascalCase")]
		public void StringToCapitalizedStringConverter_ToPascalCase()
		{
			foreach (var @case in testCases)
			{
				var res = new StringToCapitalizedStringConverter { Capitalization = StringCapitalization.ToPascalCase }.Convert(@case, CultureInfo.CurrentCulture);
				Assert.Equal("TestString", res);
			}
		}
	}
}
