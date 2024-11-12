using System.ComponentModel.DataAnnotations;

namespace Fuxion.ComponentModel.DataAnnotations;

public class HostnameAttribute : RegularExpressionAttribute
{
	public HostnameAttribute() : base(@"^([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])(\.([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]{0,61}[a-zA-Z0-9]))*$") { }
}