using System.Collections.Generic;
namespace Fuxion.Identity
{
	public interface IRol
	{
		string Name { get; }
		IEnumerable<IGroup> Groups { get; }
		IEnumerable<IPermission> Permissions { get; }
	}
}
