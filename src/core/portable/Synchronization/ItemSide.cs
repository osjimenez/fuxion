using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal class ItemSide<TSideItem, TKey> : IItemSide
    {
        public ItemSide(ISideRunner side, string name, TKey key, TSideItem sideItem, string sideItemName)
        {
            //Id = id;
            Side = side;
            Name = name;
            Key = key;
            SideItem = sideItem;
            SideItemName = sideItemName;
        }
        //public Guid Id { get; }
        public string Name { get; }
        public TKey Key { get; }
        object IItemSide.Key { get { return Key; } }
        public TSideItem SideItem { get; }
        object IItemSide.SideItem { get { return SideItem; } }
        public string SideItemName { get; }
        public ISideRunner Side { get; set; }
        public IEnumerable<IProperty> Properties { get; set; } = new List<IProperty>();
        public ICollection<IItem> SubItems { get; set; } = new List<IItem>();
    }
}
