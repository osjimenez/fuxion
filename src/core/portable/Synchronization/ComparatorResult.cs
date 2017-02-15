using System;
using System.Collections.Generic;
using System.Linq;

namespace Fuxion.Synchronization
{
    internal class ComparatorResult<TItemA, TItemB, TKey> : IComparatorResultInternal
    {
        public TKey Key { get; set; }
        object IComparatorResultInternal.Key { get { return Key; } }
        public TItemA MasterItem { get; set; }
        object IComparatorResultInternal.MasterItem { get { return MasterItem; } }
        public TItemB SideItem { get; set; }
        object IComparatorResultInternal.SideItem { get { return SideItem; } }
        public ICollection<IPropertyRunner> Properties { get; } = new List<IPropertyRunner>();
        public ICollection<ISideRunner> SubSides { get; set; } = new List<ISideRunner>();
        public void AddProperty<TPropertyA, TPropertyB>(string propertyName, TPropertyA aValue, TPropertyB bValue)
        {
            Properties.Add(new PropertyRunner<TPropertyA, TPropertyB>(propertyName, aValue, bValue));
        }
    }
}