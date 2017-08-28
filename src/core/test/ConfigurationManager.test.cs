using Fuxion.ComponentModel;
using Fuxion.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Test
{
    public class ConfigurationManagerTest
    {
        string xmlPath = @"G:\Dev\Fuxion\Repo\src\core\test\bin\Debug\config.xml";
        string jsonPath = @"G:\Dev\Fuxion\Repo\src\core\test\bin\Debug\config.json";

        private void TestFile(Func<IConfigurationManager> managerCreator)
        {
            var saved = false;
            var cleared = false;

            var man = managerCreator();
            // Create configurations
            var cfg = man.Get<ConfigurationMock>();
            cfg.IPAddress = "192.168.1.1";
            cfg.Port = 1234;
            cfg.Login = new LoginConfigurationMock
            {
                Username = "root",
                Password = "1234"
            };
            var mod = man.Get<ModuleConfigurationMock>();
            mod.LicenseId = Guid.NewGuid();
            mod.LicenseFor = "Waf Estructuras Digitales SL";
            // Save
            man.Saved += (s, e) => saved = true;
            man.Save();
            Assert.True(saved);
            // Load configuration in other manager
            var man2 = managerCreator();
            var cfg2 = man2.Get<ConfigurationMock>();
            var mod2 = man2.Get<ModuleConfigurationMock>();
            // Compare values
            Assert.Equal(cfg.IPAddress, cfg2.IPAddress);
            Assert.Equal(cfg.Port, cfg2.Port);
            Assert.Equal(cfg.Login.Username, cfg2.Login.Username);
            Assert.Equal(cfg.Login.Password, cfg2.Login.Password);
            Assert.Equal(cfg.Login.Password, cfg2.Login.Password);
            Assert.Equal(mod.LicenseId, mod2.LicenseId);
            Assert.Equal(mod.LicenseFor, mod2.LicenseFor);
            // Reset a config
            man.Reset<ConfigurationMock>();
            var cfg3 = man.Get<ConfigurationMock>();
            Assert.Equal(1111, cfg3.Port);
            // Set a config
            man.Set(cfg);
            cfg = man.Get<ConfigurationMock>();
            Assert.Equal(1234,cfg.Port);
            // Clear
            man.Cleared += (s, e) => cleared = true;
            man.Clear();
            Assert.True(cleared);
        }

        [Fact(DisplayName = "ConfigurationManager - XML")]
        public void XmlFileConfiguration()
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
            TestFile(() => new XmlFileConfiguration
            {
                Path = xmlPath
            });
        }
        [Fact(DisplayName = "ConfigurationManager - JSON")]
        public void JsonFileConfiguration()
        {
            if (File.Exists(jsonPath)) File.Delete(jsonPath);
            TestFile(() => new JsonFileConfiguration
            {
                Path = jsonPath
            });
        }
    }
    [DataContract]
    public class ConfigurationMock : ConfigurationItem<ConfigurationMock>
    {
        public override Guid ConfigurationItemId { get { return Guid.Parse("{10000000-0000-0000-0000-000000000000}"); } }
        [DataMember]
        public string IPAddress { get { return GetValue<string>(); } set { SetValue(value); } }
        [DataMember]
        public int Port { get { return GetValue(() => 1111); } set { SetValue(value); } }
        [DataMember]
        public LoginConfigurationMock Login { get { return GetValue<LoginConfigurationMock>(); } set { SetValue(value); } }
    }
    [DataContract]
    public class LoginConfigurationMock : ConfigurationItem<LoginConfigurationMock>
    {
        public override Guid ConfigurationItemId { get { return Guid.Parse("{20000000-0000-0000-0000-000000000000}"); } }
        [DataMember]
        public string Username { get { return GetValue<string>(); } set { SetValue(value); } }
        [DataMember]
        public string Password { get { return GetValue<string>(); } set { SetValue(value); } }
    }
    [DataContract]
    public class ModuleConfigurationMock : ConfigurationItem<ModuleConfigurationMock>
    {
        public override Guid ConfigurationItemId { get { return Guid.Parse("{30000000-0000-0000-0000-000000000000}"); } }
        [DataMember]
        public string LicenseFor { get { return GetValue<string>(); } set { SetValue(value); } }
        [DataMember]
        public Guid LicenseId { get { return GetValue<Guid>(); } set { SetValue(value); } }
    }
}
