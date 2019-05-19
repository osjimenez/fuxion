using System.Collections.Generic;
using System.Diagnostics;

namespace Fuxion.Synchronization
{
	[DebuggerDisplay("{" + nameof(Item) + "}")]
	internal class LoadedItem
	{
		public LoadedItem(object? item, ICollection<ISideRunner> sides)
		{
			Item = item;
			Sides = sides;
		}
		public object? Item { get; set; }
		public ICollection<ISideRunner> Sides { get; set; }
	}
}
