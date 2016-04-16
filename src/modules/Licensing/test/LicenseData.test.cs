using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Licensing.Test
{
    public class LicenseDataTest
    {
        public LicenseDataTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        [Fact]
        public void LicenseData_First()
        {
            var hardwareId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var content = new LicenseMock(
            
             hardwareId,
            productId
            );
            //var ld = new LicenseData(content);
            //output.WriteLine("ToJson:");
            //var json = ld.ToJson();
            //output.WriteLine(json);
            //output.WriteLine("FromJson:");
            //var ld2 = json.FromJson<LicenseData>();
            //var json2 = ld2.ToJson();
            //output.WriteLine(json2);

        }
    }
}
