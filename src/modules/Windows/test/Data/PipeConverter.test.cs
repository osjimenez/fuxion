using Fuxion.Test;
using Fuxion.Windows.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test.Data
{
	public class PipeConverterTest : BaseTest
	{
		public PipeConverterTest(ITestOutputHelper output) : base(output) { }
		[Fact(DisplayName = "PipeConverter - First")]
		public void First()
		{
			PipeConverter con = new PipeConverter();
			con.Converters.Add(new BooleanToNegateBooleanConverter());
			con.Converters.Add(new BooleanToVisibilityConverter());
			var res = con.Convert(false, typeof(bool), null, CultureInfo.CurrentCulture);
			Assert.Equal(Visibility.Visible, res);
		}
	}
}
