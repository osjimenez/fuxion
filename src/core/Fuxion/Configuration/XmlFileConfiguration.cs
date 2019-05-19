using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Fuxion.Configuration
{
	public class XmlFileConfiguration : IConfigurationManager
	{
		public XmlFileConfiguration()
		{
			Load();
		}

		public event EventHandler Saved;
		public event EventHandler Cleared;

		string? path;
		public string Path
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(path)) return path;
				var ass = Assembly.GetEntryAssembly() ?? Assembly.GetAssembly(typeof(XmlFileConfiguration));
				return path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ass.Location), "config.xml");
			}
			set
			{
				path = value;
				Clear();
				Load();
			}
		}
		private void Load()
		{
			if (File.Exists(Path))
			{
				var xdoc = XDocument.Load(Path);
				foreach (var e in xdoc.Root.Elements("ConfigurationItem"))
					elements.Add(new Guid(e.Attribute("id").Value), e.Descendants().First());
			}
		}
		public bool Save()
		{
			DumpObjects();
			var xdoc = new XDocument(new XElement("Configuration"));
			foreach (var pair in elements)
			{
				xdoc.Root.Add(new XElement("ConfigurationItem",
							new XAttribute("id", pair.Key),
							pair.Value));
			}
			xdoc.Save(Path);
			Saved?.Invoke(this, EventArgs.Empty);
			return true;
		}
		public void Clear()
		{
			elements.Clear();
			objects.Clear();
			Cleared?.Invoke(this, EventArgs.Empty);
		}
		private void DumpObjects()
		{
			foreach (var pair in objects)
			{
				elements.Remove(pair.Key);
				elements.Add(pair.Key, pair.Value.ToXElement(pair.Value.GetType()));
			}
		}
		Dictionary<Guid, XElement> elements = new Dictionary<Guid, XElement>();
		Dictionary<Guid, object> objects = new Dictionary<Guid, object>();
		public TConfigurationItem Get<TConfigurationItem>() where TConfigurationItem : ConfigurationItem<TConfigurationItem>, new()
		{
			var id = new TConfigurationItem().ConfigurationItemId;
			if (objects.ContainsKey(id)) return (objects[id] as TConfigurationItem)!;
			if (elements.ContainsKey(id))
			{
				var obj = elements[id].FromXElement<TConfigurationItem>();
				objects.Add(id, obj);
				return obj;
			}
			else
			{
				var res = new TConfigurationItem();
				elements.Add(res.ConfigurationItemId, res.ToXElement<TConfigurationItem>());
				objects.Add(id, res);
				return res;
			}
		}
		public void Set<TConfigurationItem>(TConfigurationItem item) where TConfigurationItem : ConfigurationItem<TConfigurationItem>, new()
		{
			objects[item.ConfigurationItemId] = item;
		}
		public TConfigurationItem Reset<TConfigurationItem>() where TConfigurationItem : ConfigurationItem<TConfigurationItem>, new()
		{
			var newConfig = new TConfigurationItem();
			// If is already instantiated, destroy it
			if (objects.ContainsKey(newConfig.ConfigurationItemId)) objects.Remove(newConfig.ConfigurationItemId);
			// If is in elements, destroy it
			if (elements.ContainsKey(newConfig.ConfigurationItemId)) elements.Remove(newConfig.ConfigurationItemId);
			// Create new and return it
			elements.Add(newConfig.ConfigurationItemId, newConfig.ToXElement<TConfigurationItem>());
			return Get<TConfigurationItem>();
		}

	}
}
