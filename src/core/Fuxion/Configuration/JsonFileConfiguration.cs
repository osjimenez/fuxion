using Fuxion.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Fuxion.Configuration
{
	public class JsonFileConfiguration : IConfigurationManager
	{
		public JsonFileConfiguration()
		{
			Load();
		}

		public event EventHandler Saved;
		public event EventHandler Cleared;

		string path = "";
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
				foreach (var con in File.ReadAllText(Path).FromJson<JsonPod<object?, Guid>[]>())
					items.Add(JsonFileConfigurationItem.FromPod(con));
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
				if (item.Pod.Is<TConfigurationItem>())
					return item.Pod.As<TConfigurationItem>();
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
				items[item.ConfigurationItemId].Pod = new JsonPod<object?, Guid>(item, item.ConfigurationItemId);
				items[item.ConfigurationItemId].Instance = null;
			}
			return item;
		}
		public bool Save()
		{
			foreach (var item in items)
				item.Pod = new JsonPod<object?, Guid>(item.Instance, item.Pod.PayloadKey);
			File.WriteAllText(path, items.Select(v => v.Pod).ToArray().ToJson());
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
		protected override Guid GetKeyForItem(JsonFileConfigurationItem item) => item.Pod.PayloadKey;
	}
	internal class JsonFileConfigurationItem
	{
		private JsonFileConfigurationItem(JsonPod<object?, Guid> pod) { Pod = pod; }

		public static JsonFileConfigurationItem FromPod(JsonPod<object?, Guid> pod) => new JsonFileConfigurationItem(pod);
		public static JsonFileConfigurationItem FromInstance(object instance, Guid id) => new JsonFileConfigurationItem(new JsonPod<object?, Guid>(instance, id))
		{
			Instance = instance
		};
		public JsonPod<object?, Guid> Pod { get; set; }
		public object? Instance { get; set; }
		//public object Instance => Pod.Payload;
	}
}
