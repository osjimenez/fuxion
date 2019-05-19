using System.Collections.Generic;

namespace Fuxion.Synchronization
{
	internal interface IComparatorResultInternal
	{
		object Key { get; }
		object? MasterItem { get; }
		object? SideItem { get; }
		ICollection<IPropertyRunner> Properties { get; set; }
		ICollection<ISideRunner> SubSides { get; set; }
	}
}
