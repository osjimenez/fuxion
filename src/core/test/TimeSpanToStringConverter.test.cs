using Fuxion.Windows.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test
{
    public class TimeSpanToStringConverterTest
    {
        public TimeSpanToStringConverterTest(ITestOutputHelper output)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("es-ES");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("es-ES");
            this.output = output;
        }
        ITestOutputHelper output;
        [Fact]
        public void TimeSpanToStringConverter_First()
        {
            var con = new TimeSpanToStringConverter();
            con.Mode = TimeSpanToStringMode.PerElements;
            con.NumberOfElements = 3;
            output.WriteLine($"3.18:53:58.1234567 => {con.Convert(TimeSpan.Parse("3.18:53:58.1234567"), null, CultureInfo.CurrentCulture)}");
            output.WriteLine($"0.18:53:58.1234567 => {con.Convert(TimeSpan.Parse("0.18:53:58.1234567"), null, CultureInfo.CurrentCulture)}");
            output.WriteLine($"0.00:53:58.1234567 => {con.Convert(TimeSpan.Parse("0.00:53:58.1234567"), null, CultureInfo.CurrentCulture)}");
        }
    }
}
