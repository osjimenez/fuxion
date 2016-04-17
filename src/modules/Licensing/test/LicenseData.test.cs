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
            var lic = new LicenseMock(hardwareId, productId);//, DateTime.UtcNow, DateTime.UtcNow);
            var data = new LicenseData(lic);
            var con = new LicenseContainer
            {
                Data = data,
                Signature = "signature"
            };
            output.WriteLine("ToJson:");
            var json = con.ToJson();
            output.WriteLine(json);

            output.WriteLine("FromJson:");
            var con2 = json.FromJson<LicenseContainer>();
            var json2 = con2.ToJson();
            output.WriteLine(json2);

        }
    }
}
