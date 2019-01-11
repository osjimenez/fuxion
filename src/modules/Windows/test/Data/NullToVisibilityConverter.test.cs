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
	public class NullToVisibilityConverterTest : BaseTest
	{
		public NullToVisibilityConverterTest(ITestOutputHelper output) : base(output) { }
		[Fact(DisplayName = "NullToVisibilityConverter - Enum value")]
		public void NullToVisibilityConverter_DisplayValue()
		{
			var res = ((IValueConverter)new NullToVisibilityConverter()).Convert(null, typeof(EnumTest?), null, CultureInfo.CurrentCulture);
			Assert.Equal(Visibility.Collapsed, res);
		}
	}
}
