using System.Diagnostics;

namespace Fuxion.Identity.Test.Dto;

[DebuggerDisplay("{" + nameof(Name) + "}")]
[TypeDiscriminated(TypeDiscriminatorIds.Base)]
public class BaseDto
{
	public BaseDto(string id, string name)
	{
		Id = id;
		Name = name;
	}
	public string Id { get; set; }
	public virtual string Name { get; set; }
}