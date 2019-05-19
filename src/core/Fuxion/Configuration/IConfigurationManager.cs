using System;

namespace Fuxion.Configuration
{
	public interface IConfigurationManager
	{
		event EventHandler Saved;
		event EventHandler Cleared;
		bool Save();
		void Clear();
		TConfigurationItem Get<TConfigurationItem>() where TConfigurationItem : ConfigurationItem<TConfigurationItem>, new();
		void Set<TConfigurationItem>(TConfigurationItem item) where TConfigurationItem : ConfigurationItem<TConfigurationItem>, new();
		TConfigurationItem Reset<TConfigurationItem>() where TConfigurationItem : ConfigurationItem<TConfigurationItem>, new();
	}
}
