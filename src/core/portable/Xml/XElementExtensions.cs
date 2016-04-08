using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace System.Xml
{
	public static class XElementExtensions
	{
		public static XElement ToXElement<T>(this object obj)
		{
			using (var memoryStream = new MemoryStream())
			{
				using (TextWriter streamWriter = new StreamWriter(memoryStream))
				{
					var xmlSerializer = new XmlSerializer(typeof(T));
					xmlSerializer.Serialize(streamWriter, obj);
                    return XElement.Parse(Encoding.UTF8.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length));
				}
			}
		}
		public static XElement ToXElement(this object obj, Type type)
		{
			using (var memoryStream = new MemoryStream())
			{
				using (TextWriter streamWriter = new StreamWriter(memoryStream))
				{
					var xmlSerializer = new XmlSerializer(type);
					xmlSerializer.Serialize(streamWriter, obj);
                    return XElement.Parse(Encoding.UTF8.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length));
                }
			}
		}
		public static T FromXElement<T>(this XElement xElement)
		{
            var str = xElement.ToString();
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(str.ToCharArray())))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                return (T)xmlSerializer.Deserialize(memoryStream);
            }
		}
	}
}
