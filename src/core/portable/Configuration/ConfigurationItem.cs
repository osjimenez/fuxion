using Fuxion.ComponentModel;
using System;
namespace Fuxion.Configuration
{
    public abstract class ConfigurationItem<TConfigurationItem> : Notifier<TConfigurationItem> where TConfigurationItem : class, INotifier<TConfigurationItem>, new()
    {
        public abstract Guid ConfigurationItemId { get; }
    }
}
