#if (NET471)
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace Fuxion.Security
{
    [Flags]
    public enum HardwareIdField
    {
        Cpu = 1,
        Bios = 2,
        Motherboard = 4,
        Disk = 8,
        Video = 16,
        Mac = 32,
        OperatingSystemSid = 64
    }
    public static class HardwareId
    {
        static Guid _cpu = Guid.Empty;
        public static Guid Cpu
        {
            get
            {
                if (_cpu == Guid.Empty)
                {
                    //Uses first CPU identifier available in order of preference
                    //Don't get all identifiers, as it is very time consuming
                    _cpu = GetHash("Win32_Processor", "UniqueId");
                    if (_cpu == Guid.Empty) //If no UniqueID, use ProcessorID
                    {
                        _cpu = GetHash("Win32_Processor", "ProcessorId");
                        if (_cpu == Guid.Empty) //If no ProcessorId, use Name
                        {
                            _cpu = GetHash("Win32_Processor", "Name");
                            if (_cpu == Guid.Empty) //If no Name, use Manufacturer
                            {
                                _cpu = GetHash("Win32_Processor", "Manufacturer");
                            }
                            //Add clock speed for extra security
                            _cpu = CombineHash(_cpu, GetHash("Win32_Processor", "MaxClockSpeed"));
                        }
                    }
                }
                return _cpu;
            }
        }
        static Guid _bios = Guid.Empty;
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
            set { _bios = value; }
        }
        static Guid _disk = Guid.Empty;
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
            set { _disk = value; }
        }
        static Guid _motherboard = Guid.Empty;
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
            set { _motherboard = value; }
        }
        static Guid _video = Guid.Empty;
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
            set { _video = value; }
        }
        static Guid _mac = Guid.Empty;
        public static Guid Mac
        {
            get
            {
                if (_mac == Guid.Empty)
                {
                    _mac = CombineHash(_mac, GetHash("Win32_NetworkAdapterConfiguration", "MACAddress", "IPEnabled"));
                }
                return _mac;
            }
            set { _mac = value; }
        }
        static Guid _operatingSystemSid = Guid.Empty;
        public static Guid OperatingSystemSid
        {
            get
            {
                if (_operatingSystemSid == Guid.Empty)
                {
                    //NTAccount account = new NTAccount(Environment.MachineName, "SYSTEM");
                    //SecurityIdentifier sid =
                    //(SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));

                    //// we're done, show the results:
                    //Console.WriteLine(account.Value);
                    //Console.WriteLine(sid.Value);
                    _operatingSystemSid = new Guid(WindowsIdentity.GetCurrent().User.AccountDomainSid.ToString().ComputeHash());
                }
                return _operatingSystemSid;
            }
            set { _operatingSystemSid = value; }
        }
        public static Guid Get(HardwareIdField fields)
        {
            Guid res = Guid.Empty;
            if (fields.HasFlag(HardwareIdField.Bios))
                res = CombineHash(res, Bios);
            if (fields.HasFlag(HardwareIdField.Cpu))
                res = CombineHash(res, Cpu);
            if (fields.HasFlag(HardwareIdField.Disk))
                res = CombineHash(res, Disk);
            if (fields.HasFlag(HardwareIdField.Mac))
                res = CombineHash(res, Mac);
            if (fields.HasFlag(HardwareIdField.Motherboard))
                res = CombineHash(res, Motherboard);
            if (fields.HasFlag(HardwareIdField.Video))
                res = CombineHash(res, Video);
            if (fields.HasFlag(HardwareIdField.OperatingSystemSid))
                res = CombineHash(res, OperatingSystemSid);
            return res;
        }
        private static Guid CombineHash(Guid guid1, Guid guid2)
        {
            var res = guid1.ToByteArray();
            res.CombineHash(guid2.ToByteArray());
            return new Guid(res);
        }
        private static void CombineHash(this byte[] me, byte[] byteArray)
        {
            if (me.Length != byteArray.Length) throw new ArgumentException("Los arrays a combinar deben tener el mismo tamaño");
            byte[] res = new byte[me.Length];
            for (int i = 0; i < me.Length; i++)
                me[i] = (byte)(me[i] ^ byteArray[i]);
        }
        private static byte[] ComputeHash(this string me)
        {
            return new MD5CryptoServiceProvider().ComputeHash(
                        new ASCIIEncoding().GetBytes(me));
        }
        private static Guid GetHash(string wmiClass, string wmiProperty, string wmiMustBeTrue = null)
        {
            var list = new List<byte[]>();
            byte[] res = new byte[16];
            System.Management.ManagementClass mc = new System.Management.ManagementClass(wmiClass);
            System.Management.ManagementObjectCollection moc = mc.GetInstances();
            foreach (System.Management.ManagementObject mo in moc)
            {
                if ((wmiMustBeTrue == null || mo[wmiMustBeTrue].ToString() == "True") && mo[wmiProperty] != null)
                {
                    list.Add(mo[wmiProperty].ToString().ComputeHash());
                }
            }
            if (list.Count == 0) return Guid.Empty;
            foreach (var g in list)
                res.CombineHash(g);
            return new Guid(res); ;
        }
    }
}
#endif