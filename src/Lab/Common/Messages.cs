using Fuxion.Domain;
using Fuxion.Reflection;

namespace Fuxion.Lab.Common;

[UriKey(UriKey.FuxionBaseUri+"lab/test-message/1.0.0")]
public class TestMessage : IMessage
{
	public TestMessage(int id, string name)
	{
		Id = id;
		Name = name;
	}
	public int Id { get; set; }
	public string Name { get; set; }
}