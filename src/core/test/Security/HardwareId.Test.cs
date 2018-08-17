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
            
			var bios = HardwareId.Bios;
			var cpu = HardwareId.Cpu;
			var disk = HardwareId.Disk;
			var mac = HardwareId.Mac;
			var motherboard = HardwareId.Motherboard;
			var domain = HardwareId.DomainSid;
			var so = HardwareId.OperatingSystemProductId;
			var video = HardwareId.Video;

			void Print(Guid value, string name)
			{
				output.WriteLine($"{name} = {value}");
				output.WriteLine($"{name}.GetHashCode() = {value.GetHashCode()}");
				output.WriteLine($"(uint){name}.GetHashCode() = {(uint)value.GetHashCode()}");
				output.WriteLine($"===============================");
			}
			Print(bios, "BIOS");
			Print(cpu, "CPU");
			Print(disk, "DISK");
			Print(mac, "MAC");
			Print(motherboard, "MOTHERBOARD");
			Print(domain, "DOMAIN");
			Print(so, "SO");
			Print(video, "VIDEO");
        }
    }
}
