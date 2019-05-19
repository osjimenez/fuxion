using System;
using System.Collections.Generic;

namespace Fuxion.Synchronization
{
	public class Session
	{
		public Session(string name)
		{
			Name = name;
		}
		internal Guid Id { get; } = Guid.NewGuid();
		public string Name { get; set; }
		public bool MakePreviewInParallel { get; set; } = false;
		public ICollection<Work> Works { get; set; } = new List<Work>();
	}
}
