using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml;

namespace Fuxion.Configuration
{
    public class EncryptedFileConfiguration : IConfigurationManager
    {
        public EncryptedFileConfiguration()
        {
            if (File.Exists(path))
            {
                var xdoc = XDocument.Load(path);
                foreach (var e in xdoc.Root.Elements("ConfigurationItem"))
                    elements.Add(new Guid(e.Attribute("id").Value), e.Descendants().First());
            }
        }
        static string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\config.bin";
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

            //EncryptedData edElement = new EncryptedData();
            //edElement.Type = EncryptedXml.XmlEncElementUrl;

            xdoc.Save(path);
            return true;
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
            if (objects.ContainsKey(id)) return objects[id] as TConfigurationItem;
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
            //Si esta instanciado lo destruyo
            if (objects.ContainsKey(newConfig.ConfigurationItemId)) objects.Remove(newConfig.ConfigurationItemId);
            //Si esta en los elementos lo destroyo
            if (elements.ContainsKey(newConfig.ConfigurationItemId)) elements.Remove(newConfig.ConfigurationItemId);
            //Ahora creo un nuevo elemento
            elements.Add(newConfig.ConfigurationItemId, newConfig.ToXElement<TConfigurationItem>());
            //Obtengo el objeto y lo devuelvo
            return Get<TConfigurationItem>();
        }
    }
}
