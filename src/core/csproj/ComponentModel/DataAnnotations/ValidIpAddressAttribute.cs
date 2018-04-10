using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fuxion.ComponentModel.DataAnnotations
{
	public class ValidIpAddressAttribute : RegularExpressionAttribute
	{
		public ValidIpAddressAttribute() : base(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$") { }
	}
}