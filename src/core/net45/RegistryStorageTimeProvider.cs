using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public class RegistryStorageTimeProvider : StorageTimeProvider
    {
        public string Key { get; set; } = @"Software\Fuxion\RegistryStorageTimeProvider";
        public string Value { get; set; } = "Value";
        public override DateTime GetUtcTime()
        {
            var key  = Registry.CurrentUser.CreateSubKey(Key);
            var value = DateTime.Parse(key.GetValue(Value).ToString());
            return value;
        }
        public override void SaveUtcTime(DateTime time)
        {
            var key = Registry.CurrentUser.CreateSubKey(Key);
            key.SetValue(Value, time.ToString());
        }
    }
}
