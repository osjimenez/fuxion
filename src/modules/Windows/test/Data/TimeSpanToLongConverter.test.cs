using Fuxion.Test;
using Fuxion.Windows.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test.Data
{
	public class TimeSpanToLongConverterTest:BaseTest
	{
		public TimeSpanToLongConverterTest(ITestOutputHelper output) : base(output) { }
		[Fact(DisplayName = "TimeSpanToLongConverter - ConvertToSeconds")]
		public void ConvertToSeconds()
		{
			var conv = new TimeSpanToLongConverter
			{
				Value = TimeSpanToLongValue.Seconds
			};

			var res = conv.Convert(TimeSpan.FromTicks(700000000), CultureInfo.CurrentCulture);
			Assert.Equal(10, res);

			res = conv.Convert(TimeSpan.FromMilliseconds(70000), CultureInfo.CurrentCulture);
			Assert.Equal(10, res);

			res = conv.Convert(TimeSpan.FromSeconds(70), CultureInfo.CurrentCulture);
			Assert.Equal(10, res);

			res = conv.Convert(TimeSpan.FromMinutes(1.18), CultureInfo.CurrentCulture);
			Assert.Equal(10, res);

			res = conv.Convert(TimeSpan.FromHours(0.0028), CultureInfo.CurrentCulture);
			Assert.Equal(10, res);

			res = conv.Convert(TimeSpan.FromDays(0.00012), CultureInfo.CurrentCulture);
			Assert.Equal(10, res);
		}
	}
}
