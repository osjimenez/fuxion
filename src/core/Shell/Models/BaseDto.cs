using System;
using System.Collections.Generic;
using System.Text;

namespace Fuxion.Shell.Models
{
	public class BaseDto
	{
		public Guid Id { get; set; }
		public Dictionary<Guid, Property> Properties { get; set; }
	}
	public class Property
	{
		public Guid Id { get; set; }
	}
}

