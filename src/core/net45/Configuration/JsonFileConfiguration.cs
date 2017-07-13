using Fuxion.ComponentModel;
using Fuxion.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.ObjectModel;

namespace Fuxion.Configuration
{
    public class JsonFileConfiguration : IConfigurationManager
    {
        public JsonFileConfiguration(string path = null)
        {
            this.path = path;
            Load();
        }

        public event EventHandler Saved;
        public event EventHandler Cleared;

        string path;
        public string Path
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(path)) return path;
                var ass = Assembly.GetEntryAssembly() ?? Assembly.GetAssembly(typeof(JsonFileConfiguration));
                return path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ass.Location), "config.json");
            }
            set
            {
                if (path != value)
                {
                    path = value;
                    Clear();
                    Load();
                }
            }
        }
        JsonFileConfigurationItemCollection items = new JsonFileConfigurationItemCollection();
        private void Load()
        {
            if (File.Exists(Path))
                foreach (var con in File.ReadAllText(Path).FromJson<JsonContainer<Guid>[]>())
                    items.Add(JsonFileConfigurationItem.FromContainer(con));
        }

        public void Clear()
        {
            items.Clear();
            Cleared?.Invoke(this, EventArgs.Empty);
        }
        public TConfigurationItem Get<TConfigurationItem>() where TConfigurationItem : ConfigurationItem<TConfigurationItem>, new()
        {
            var id = new TConfigurationItem().ConfigurationItemId;
            if (items.Contains(id))
            {
                var item = items[id];
                if (item.Instance != null)
                    return (TConfigurationItem)item.Instance;
                if (item.Container.Is<TConfigurationItem>())
                    return item.Container.As<TConfigurationItem>();
                throw new InvalidCastException($"Configuration item with id '{id}' is not of type '{typeof(TConfigurationItem).Name}'");
            }
            else
            {
                var res = new TConfigurationItem();
                AddItem(res);
                return res;
            }
        }
        private void AddItem<TConfigurationItem>(TConfigurationItem item) where TConfigurationItem : ConfigurationItem<TConfigurationItem>, new()
            => items.Add(JsonFileConfigurationItem.FromInstance(item, item.ConfigurationItemId));

        public TConfigurationItem Reset<TConfigurationItem>() where TConfigurationItem : ConfigurationItem<TConfigurationItem>, new()
        {
            var item = new TConfigurationItem();
            if (items.Contains(item.ConfigurationItemId))
            {
                items[item.ConfigurationItemId].Container = JsonContainer<Guid>.Create(item, item.ConfigurationItemId);
                items[item.ConfigurationItemId].Instance = null;
            }
            return item;
        }
        public bool Save()
        {
            foreach (var item in items)
                item.Container = JsonContainer<Guid>.Create(item.Instance, item.Container.Key);
            File.WriteAllText(path, items.Select(v => v.Container).ToArray().ToJson());
            Saved?.Invoke(this, EventArgs.Empty);
            return true;
        }
        public void Set<TConfigurationItem>(TConfigurationItem item) where TConfigurationItem : ConfigurationItem<TConfigurationItem>, new()
        {
            if (items.Contains(item.ConfigurationItemId))
            {
                var actual = items[item.ConfigurationItemId];
                items.Remove(item.ConfigurationItemId);
            }
            AddItem(item);
        }
    }
    internal class JsonFileConfigurationItemCollection : KeyedCollection<Guid, JsonFileConfigurationItem>
    {
        protected override Guid GetKeyForItem(JsonFileConfigurationItem item) => item.Container.Key;
    }
    internal class JsonFileConfigurationItem
    {
        private JsonFileConfigurationItem() { }

        public static JsonFileConfigurationItem FromContainer(JsonContainer<Guid> container) => new JsonFileConfigurationItem
        {
            Container = container,
        };
        public static JsonFileConfigurationItem FromInstance(object instance, Guid id) => new JsonFileConfigurationItem
        {
            Container = JsonContainer<Guid>.Create(instance, id),
            Instance = instance
        };
        public JsonContainer<Guid> Container { get; set; }
        public object Instance { get; set; }
    }
}
