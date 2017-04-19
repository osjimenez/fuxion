using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal class ItemSideRunner<TSideItem, TKey> : IItemSideRunner
    {
        public ItemSideRunner(ISideRunner side, string name, TKey key, TSideItem sideItem, string sideItemName, string sideItemTag)
        {
            Side = side;
            Name = name;
            Key = key;
            SideItem = sideItem;
            SideItemName = sideItemName;
            SideItemTag = sideItemTag;
        }
        public string Name { get; }
        public TKey Key { get; }
        object IItemSideRunner.Key { get { return Key; } }
        public TSideItem SideItem { get; }
        object IItemSideRunner.SideItem { get { return SideItem; } }
        public string SideItemName { get; }
        public string SideItemTag { get; }
        public ISideRunner Side { get; set; }
        public IEnumerable<IPropertyRunner> Properties { get; set; } = new List<IPropertyRunner>();
        public ICollection<IItemRunner> SubItems { get; set; } = new List<IItemRunner>();
    }
}
