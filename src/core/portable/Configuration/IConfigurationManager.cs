using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fuxion.Configuration
{
    public interface IConfigurationManager
    {
        bool Save();
        void Clear();
        TConfigurationItem Get<TConfigurationItem>() where TConfigurationItem : ConfigurationItem<TConfigurationItem>, new();
        void Set<TConfigurationItem>(TConfigurationItem item) where TConfigurationItem : ConfigurationItem<TConfigurationItem>, new();
        TConfigurationItem Reset<TConfigurationItem>() where TConfigurationItem : ConfigurationItem<TConfigurationItem>, new();
    }
}
