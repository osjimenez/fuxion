using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace System.Xml;

public static class XElementExtensions
{
	public static XElement ToXElement<T>(this T obj)
	{
		using var memoryStream = new MemoryStream();
		using var streamWriter = new StreamWriter(memoryStream);
		var xmlSerializer = new XmlSerializer(typeof(T));
		xmlSerializer.Serialize(streamWriter, obj);
		return XElement.Parse(Encoding.UTF8.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length));
	}
	public static XElement ToXElement(this object obj, Type type)
	{
		using var memoryStream = new MemoryStream();
		using TextWriter streamWriter = new StreamWriter(memoryStream);
		var xmlSerializer = new XmlSerializer(type);
		xmlSerializer.Serialize(streamWriter, obj);
		return XElement.Parse(Encoding.UTF8.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length));
	}
	public static T FromXElement<T>(this XElement xElement)
	{
		using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xElement.ToString().ToCharArray()));
		var xmlSerializer = new XmlSerializer(typeof(T));
		return (T)(xmlSerializer.Deserialize(memoryStream) ?? throw new InvalidCastException($"Deserialization from type '{typeof(T).Name}' failed"));
	}
}