using System.ComponentModel.DataAnnotations;

namespace Fuxion.ComponentModel.DataAnnotations {
	public class ValidHostnameAttribute : RegularExpressionAttribute
	{
		public ValidHostnameAttribute() : base(@"^([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])(\.([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]{0,61}[a-zA-Z0-9]))*$") { }
	}
}