using Fuxion.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test.Security
{
    public class HardwareIdTest : BaseTest
    {
        public HardwareIdTest(ITestOutputHelper output) : base(output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        [Fact(DisplayName = "HardwareId - Printable version of ids")]
        public void PrintableVersion()
        {
            var id = HardwareId.Cpu;
            output.WriteLine("id = " + id);
            output.WriteLine("id.GetHashCode() = " + id.GetHashCode());
            output.WriteLine("(uint)id.GetHashCode() = " + (uint)id.GetHashCode());

            id = Guid.NewGuid();
            output.WriteLine("id = " + id);
            output.WriteLine("id.GetHashCode() = " + id.GetHashCode());
            output.WriteLine("(uint)id.GetHashCode() = " + (uint)id.GetHashCode());

            id = Guid.NewGuid();
            output.WriteLine("id = " + id);
            output.WriteLine("id.GetHashCode() = " + id.GetHashCode());
            output.WriteLine("(uint)id.GetHashCode() = " + (uint)id.GetHashCode());

            id = Guid.NewGuid();
            output.WriteLine("id = " + id);
            output.WriteLine("id.GetHashCode() = " + id.GetHashCode());
            output.WriteLine("(uint)id.GetHashCode() = " + (uint)id.GetHashCode());
        }
    }
}
