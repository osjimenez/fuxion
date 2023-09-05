using Fuxion.Xunit;
using Fuxion.Windows.Security;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Windows.Test.Security;

public class HardwareIdTest : BaseTest<HardwareIdTest>
{
	public HardwareIdTest(ITestOutputHelper output) : base(output) { }
	[Fact(DisplayName = "HardwareId - Printable version of ids")]
	public void PrintableVersion()
	{
		var bios = HardwareId.Bios;
		var cpu = HardwareId.Cpu;
		var disk = HardwareId.Disk;
		var mac = HardwareId.Mac;
		var motherboard = HardwareId.Motherboard;
		var so = HardwareId.OperatingSystemProductId;
		var video = HardwareId.Video;
		void Print(Guid value, string name)
		{
			Output.WriteLine($"{name} = {value}");
			Output.WriteLine($"{name}.GetHashCode() = {value.GetHashCode()}");
			Output.WriteLine($"(uint){name}.GetHashCode() = {(uint)value.GetHashCode()}");
			Output.WriteLine("===============================");
		}
		Print(bios, "BIOS");
		Print(cpu, "CPU");
		Print(disk, "DISK");
		Print(mac, "MAC");
		Print(motherboard, "MOTHERBOARD");
		Print(so, "SO");
		Print(video, "VIDEO");
	}
}