using Fuxion.ComponentModel;
using Fuxion.Configuration;
using System;
using System.IO;
using System.Runtime.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test.Configuration
{
	public class ConfigurationManagerTest : BaseTest
	{
		public ConfigurationManagerTest(ITestOutputHelper output) : base(output) { }

		private string xmlPath = @"config.xml";
		private string jsonPath = @"config.json";

		private void TestFile(Func<IConfigurationManager> managerCreator)
		{
			bool saved = false;
			bool cleared = false;

			IConfigurationManager man = managerCreator();
			// Create configurations
			ConfigurationMock cfg = man.Get<ConfigurationMock>();
			cfg.IPAddress = "192.168.1.1";
			cfg.Port = 1234;
			cfg.Login = new LoginConfigurationMock
			{
				Username = "root",
				Password = "1234"
			};
			ModuleConfigurationMock mod = man.Get<ModuleConfigurationMock>();
			mod.LicenseId = Guid.NewGuid();
			mod.LicenseFor = "Waf Estructuras Digitales SL";
			// Save
			man.Saved += (s, e) => saved = true;
			man.Save();
			Assert.True(saved);
			// Load configuration in other manager
			IConfigurationManager man2 = managerCreator();
			ConfigurationMock cfg2 = man2.Get<ConfigurationMock>();
			ModuleConfigurationMock mod2 = man2.Get<ModuleConfigurationMock>();
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
			ConfigurationMock cfg3 = man.Get<ConfigurationMock>();
			Assert.Equal(1111, cfg3.Port);
			// Set a config
			man.Set(cfg);
			cfg = man.Get<ConfigurationMock>();
			Assert.Equal(1234, cfg.Port);
			// Clear
			man.Cleared += (s, e) => cleared = true;
			man.Clear();
			Assert.True(cleared);
		}

		[Fact(DisplayName = "ConfigurationManager - XML")]
		public void XmlFileConfiguration()
		{
			if (File.Exists(xmlPath))
			{
				File.Delete(xmlPath);
			}

			TestFile(() => new XmlFileConfiguration
			{
				Path = xmlPath
			});
		}
		[Fact(DisplayName = "ConfigurationManager - JSON")]
		public void JsonFileConfiguration()
		{
			if (File.Exists(jsonPath))
			{
				File.Delete(jsonPath);
			}
			TestFile(() => new JsonFileConfiguration
			{
				Path = jsonPath
			});
		}
	}
	[DataContract]
	public class ConfigurationMock : ConfigurationItem<ConfigurationMock>
	{
		public override Guid ConfigurationItemId => Guid.Parse("{10000000-0000-0000-0000-000000000000}");
		[DataMember]
		public string IPAddress { get => GetValue<string>(); set => SetValue(value); }
		[DataMember]
		public int Port { get => GetValue(() => 1111); set => SetValue(value); }
		[DataMember]
		public LoginConfigurationMock Login { get => GetValue<LoginConfigurationMock>(); set => SetValue(value); }
	}
	[DataContract]
	public class LoginConfigurationMock : ConfigurationItem<LoginConfigurationMock>
	{
		public override Guid ConfigurationItemId => Guid.Parse("{20000000-0000-0000-0000-000000000000}");
		[DataMember]
		public string Username { get => GetValue<string>(); set => SetValue(value); }
		[DataMember]
		public string Password { get => GetValue<string>(); set => SetValue(value); }
	}
	[DataContract]
	public class ModuleConfigurationMock : ConfigurationItem<ModuleConfigurationMock>
	{
		public override Guid ConfigurationItemId => Guid.Parse("{30000000-0000-0000-0000-000000000000}");
		[DataMember]
		public string LicenseFor { get => GetValue<string>(); set => SetValue(value); }
		[DataMember]
		public Guid LicenseId { get => GetValue<Guid>(); set => SetValue(value); }
	}

}
