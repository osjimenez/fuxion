using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public class RegistryStorageTimeProvider : StoredTimeProvider
    {
        public string Key { get; set; } = @"Software\Fuxion\RegistryStorageTimeProvider";
        public string Value { get; set; } = "Value";
        public bool EncryptValue { get; set; }

        public override DateTime GetUtcTime()
        {
            var key  = Registry.CurrentUser.CreateSubKey(Key);
            var value = Deserialize(key.GetValue(Value).ToString());
            //var value = DateTime.Parse(key.GetValue(Value).ToString());
            return value;
        }
        public override void SaveUtcTime(DateTime time)
        {
            var key = Registry.CurrentUser.CreateSubKey(Key);
            //key.SetValue(Value, time.ToString());
            key.SetValue(Value, Serialize(time));
        }
        static byte[] entropy = { 9, 8, 7, 6, 5 };
        protected override string Serialize(DateTime time)
        {
            return EncryptValue
                ? Encoding.Default.GetString(ProtectedData.Protect(Encoding.Default.GetBytes(time.ToString()), entropy, DataProtectionScope.CurrentUser))
                : base.Serialize(time);
        }
        protected override DateTime Deserialize(string value)
        {
            return EncryptValue
                ? DateTime.Parse(Encoding.Default.GetString(ProtectedData.Unprotect(Encoding.Default.GetBytes(value), entropy, DataProtectionScope.CurrentUser)))
                : base.Deserialize(value);
        }
    }
}
