using Fuxion.Domain;
using Fuxion.Reflection;

namespace Fuxion.Lab.Common;

[TypeKey("test-message")]
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