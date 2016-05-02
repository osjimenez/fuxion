using Fuxion.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Test
{
    public class XmlFileConfigurationTest
    {
        [Fact]
        public void XmlFileConfiguration_First()
        {
            XmlFileConfiguration f = new XmlFileConfiguration();
            var cm = f.Get<ConfigurationMock>();
            cm.String = "Test";
            f.Save();
        }
    }
    public class ConfigurationMock : ConfigurationItem<ConfigurationMock>
    {
        public override Guid ConfigurationItemId { get { return Guid.Parse("{7476B8AD-E6EB-4379-89D3-A5777013E9D1}"); } }
        public string String { get { return GetValue<string>(); } set { SetValue(value); } }
    }
}
