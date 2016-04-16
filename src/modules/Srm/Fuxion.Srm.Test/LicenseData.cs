using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Srm.Test
{
    public class LicenseData
    {
        public LicenseData(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        [Fact]
        public void LicenseData_First()
        {
            var content = new LicenseContentMock
            {
                HardwareId = Guid.NewGuid(),
                ProductId = Guid.NewGuid()
            };
            var ld = new Fuxion.Srm.LicenseData(content);
            output.WriteLine("ToJson:");
            var json = ld.ToJson();
            output.WriteLine(json);
            output.WriteLine("FromJson:");
            var ld2 = json.FromJson<Srm.LicenseData>();
            output.WriteLine(ld2.Content.ToString());

        }
    }
}
