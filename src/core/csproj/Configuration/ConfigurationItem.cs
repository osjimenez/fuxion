using Fuxion.ComponentModel;
using Newtonsoft.Json;
using System;
namespace Fuxion.Configuration
{
    public abstract class ConfigurationItem<TConfigurationItem> : Notifier<TConfigurationItem> where TConfigurationItem : class, INotifier<TConfigurationItem>, new()
    {
        [JsonIgnore]
        public abstract Guid ConfigurationItemId { get; }
    }
}
