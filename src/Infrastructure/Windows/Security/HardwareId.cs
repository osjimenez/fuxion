using System.Management;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace Fuxion.Windows.Security;

[Flags]
public enum HardwareIdField
{
	Cpu         = 1,
	Bios        = 2,
	Motherboard = 4,
	Disk        = 8,
	Video       = 16,
	Mac         = 32
}

public static class HardwareId
{
	static Guid _cpu                      = Guid.Empty;
	static Guid _bios                     = Guid.Empty;
	static Guid _disk                     = Guid.Empty;
	static Guid _motherboard              = Guid.Empty;
	static Guid _video                    = Guid.Empty;
	static Guid _mac                      = Guid.Empty;
	static Guid _operatingSystemProductId = Guid.Empty;
	public static Guid Cpu
	{
		get
		{
			if (_cpu == Guid.Empty)
			{
				// https://www.codeproject.com/Articles/28678/Generating-Unique-Key-Finger-Print-for-a-Computer
				//_cpu = CombineHash(_cpu, GetHash("Win32_Processor", "UniqueId"));
				//_cpu = CombineHash(_cpu, GetHash("Win32_Processor", "ProcessorId"));
				//_cpu = CombineHash(_cpu, GetHash("Win32_Processor", "Name"));
				//_cpu = CombineHash(_cpu, GetHash("Win32_Processor", "Manufacturer"));
				//_cpu = CombineHash(_cpu, GetHash("Win32_Processor", "MaxClockSpeed"));

				//Uses first CPU identifier available in order of preference
				//Don't get all identifiers, as it is very time consuming
				_cpu = GetHash("Win32_Processor", "UniqueId");
				if (_cpu == Guid.Empty) //If no UniqueID, use ProcessorID
					_cpu = GetHash("Win32_Processor", "ProcessorId");
				//if (_cpu == Guid.Empty) //If no ProcessorId, use Name
				//{
				//	_cpu = GetHash("Win32_Processor", "Name");
				//	if (_cpu == Guid.Empty) //If no Name, use Manufacturer
				//	{
				//		_cpu = GetHash("Win32_Processor", "Manufacturer");
				//	}
				//	//Add clock speed for extra security
				//	_cpu = CombineHash(_cpu, GetHash("Win32_Processor", "MaxClockSpeed"));
				//}
			}
			return _cpu;
		}
	}
	public static Guid Bios
	{
		get
		{
			if (_bios == Guid.Empty)
			{
				_bios = CombineHash(_bios, GetHash("Win32_BIOS", "Manufacturer"));
				_bios = CombineHash(_bios, GetHash("Win32_BIOS", "SMBIOSBIOSVersion"));
				_bios = CombineHash(_bios, GetHash("Win32_BIOS", "IdentificationCode"));
				_bios = CombineHash(_bios, GetHash("Win32_BIOS", "SerialNumber"));
				_bios = CombineHash(_bios, GetHash("Win32_BIOS", "ReleaseDate"));
				_bios = CombineHash(_bios, GetHash("Win32_BIOS", "Version"));
			}
			return _bios;
		}
		set => _bios = value;
	}
	public static Guid Disk
	{
		get
		{
			if (_disk == Guid.Empty)
			{
				_disk = CombineHash(_disk, GetHash("Win32_DiskDrive", "Model"));
				_disk = CombineHash(_disk, GetHash("Win32_DiskDrive", "Manufacturer"));
				_disk = CombineHash(_disk, GetHash("Win32_DiskDrive", "Signature"));
				_disk = CombineHash(_disk, GetHash("Win32_DiskDrive", "TotalHeads"));
			}
			return _disk;
		}
		set => _disk = value;
	}
	public static Guid Motherboard
	{
		get
		{
			if (_motherboard == Guid.Empty)
			{
				_motherboard = CombineHash(_motherboard, GetHash("Win32_BaseBoard", "Model"));
				_motherboard = CombineHash(_motherboard, GetHash("Win32_BaseBoard", "Manufacturer"));
				_motherboard = CombineHash(_motherboard, GetHash("Win32_BaseBoard", "Name"));
				_motherboard = CombineHash(_motherboard, GetHash("Win32_BaseBoard", "SerialNumber"));
			}
			return _motherboard;
		}
		set => _motherboard = value;
	}
	public static Guid Video
	{
		get
		{
			if (_video == Guid.Empty)
			{
				_video = CombineHash(_video, GetHash("Win32_VideoController", "DriverVersion"));
				_video = CombineHash(_video, GetHash("Win32_VideoController", "Name"));
			}
			return _video;
		}
		set => _video = value;
	}
	public static Guid Mac
	{
		get
		{
			if (_mac == Guid.Empty) _mac = CombineHash(_mac, GetHash("Win32_NetworkAdapterConfiguration", "MACAddress", "IPEnabled"));
			return _mac;
		}
		set => _mac = value;
	}
	public static Guid OperatingSystemProductId
	{
		get
		{
			if (_operatingSystemProductId == Guid.Empty)
			{
				string? value64  = null;
				var     localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
				localKey = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
				if (localKey is not null) value64 = localKey.GetValue("ProductId")?.ToString();
				Console.WriteLine("RegisteredOrganization [value64]: {0}", value64);

				//var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion");
				//var val = key.GetValue("ProductId");
				_operatingSystemProductId = new(ComputeHash(value64 ?? throw new InvalidStateException("Value cannot be retrieved from registry")));
			}
			return _operatingSystemProductId;
		}
		set => _operatingSystemProductId = value;
	}
	public static Guid Get(HardwareIdField fields)
	{
		var res                                              = Guid.Empty;
		if (fields.HasFlag(HardwareIdField.Bios)) res        = CombineHash(res, Bios);
		if (fields.HasFlag(HardwareIdField.Cpu)) res         = CombineHash(res, Cpu);
		if (fields.HasFlag(HardwareIdField.Disk)) res        = CombineHash(res, Disk);
		if (fields.HasFlag(HardwareIdField.Mac)) res         = CombineHash(res, Mac);
		if (fields.HasFlag(HardwareIdField.Motherboard)) res = CombineHash(res, Motherboard);
		if (fields.HasFlag(HardwareIdField.Video)) res       = CombineHash(res, Video);
		return res;
	}
	static Guid CombineHash(Guid guid1, Guid guid2)
	{
		var res = guid1.ToByteArray();
		res.CombineHash(guid2.ToByteArray());
		return new(res);
	}
	static void CombineHash(this byte[] me, byte[] byteArray)
	{
		if (me.Length != byteArray.Length) throw new ArgumentException("Los arrays a combinar deben tener el mismo tamaño");
		var res                                   = new byte[me.Length];
		for (var i = 0; i < me.Length; i++) me[i] = (byte)(me[i] ^ byteArray[i]);
	}
	static byte[] ComputeHash(this string me) => MD5.Create().ComputeHash(new ASCIIEncoding().GetBytes(me));
	static Guid GetHash(string wmiClass, string wmiProperty, string? wmiMustBeTrue = null)
	{
		var list = new List<byte[]>();
		var res  = new byte[16];
		var mc   = new ManagementClass(wmiClass);
		var moc  = mc.GetInstances();
		foreach (ManagementObject mo in moc)
			if ((wmiMustBeTrue == null || mo[wmiMustBeTrue].ToString() == "True") && mo[wmiProperty] != null)
			{
				var pro = mo[wmiProperty].ToString();
				if (pro == null) throw new InvalidProgramException("Error reading WMI property");
				list.Add(pro.ComputeHash());
			}
		if (list.Count == 0) return Guid.Empty;
		foreach (var g in list) res.CombineHash(g);
		return new(res);
		;
	}
}