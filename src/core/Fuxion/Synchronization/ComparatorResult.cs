using System.Collections.Generic;

namespace Fuxion.Synchronization
{
	internal class ComparatorResult<TItemA, TItemB, TKey> : IComparatorResultInternal
	{
		public ComparatorResult(TKey key, TItemA masterItem, TItemB sideItem)
		{
			Key = key;
			MasterItem = masterItem;
			SideItem = sideItem;
		}
		public TKey Key { get; set; }
		object IComparatorResultInternal.Key { get { return Key!; } }
		public TItemA MasterItem { get; set; }
		object? IComparatorResultInternal.MasterItem { get { return MasterItem!; } }
		public TItemB SideItem { get; set; }
		object? IComparatorResultInternal.SideItem { get { return SideItem!; } }
		public ICollection<IPropertyRunner> Properties { get; set; } = new List<IPropertyRunner>();
		public ICollection<ISideRunner> SubSides { get; set; } = new List<ISideRunner>();
	}
}