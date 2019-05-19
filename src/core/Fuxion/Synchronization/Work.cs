using System;
using System.Collections.Generic;
namespace Fuxion.Synchronization
{
	public class Work
	{
		public Work(string name, IEnumerable<ISide> sides, IEnumerable<IComparator> comparators)
		{
			Name = name;
			Sides = sides;
			Comparators = comparators;
		}
		public Guid Id { get; } = Guid.NewGuid();
		public string Name { get; set; }
		public bool LoadSidesInParallel { get; set; } = false;
		public IEnumerable<ISide> Sides { get; set; }
		public IEnumerable<IComparator> Comparators { get; set; }
		public Action<SessionPreview>? PostPreviewAction { get; set; }
		public Action<SessionPreview>? PostRunAction { get; set; }
	}
}