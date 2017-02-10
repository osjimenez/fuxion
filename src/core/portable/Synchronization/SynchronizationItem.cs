using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal interface ISynchronizationItem
    {
        Guid Id { get; }
        object MasterItem { get; }
        string MasterName { get; }
        IEnumerable<ISynchronizationItemSide> Sides { get; }
    }
    internal class SynchronizationItem<TMasterItem> : ISynchronizationItem
    {
        public SynchronizationItem(TMasterItem masterItem, string masterName)
        {
            MasterItem = masterItem;
            MasterName = masterName;
        }
        public Guid Id { get; } = Guid.NewGuid();
        public string MasterName { get; }
        public TMasterItem MasterItem { get; }
        object ISynchronizationItem.MasterItem { get { return MasterItem; } }
        public IEnumerable<ISynchronizationItemSide> Sides { get; set; } = new List<ISynchronizationItemSide>();
    }
    internal interface ISynchronizationItemSide
    {
        Guid Id { get; }
        string Name { get; }
        object Key { get; }
        object SideItem { get; }
        string SideItemName { get; }
        IEnumerable<ISynchronizationProperty> Properties { get; }
        ICollection<ISynchronizationItem> SubItems { get; set; }
    }
    internal class SynchronizationItemSide<TSideItem, TKey> : ISynchronizationItemSide
    {
        public SynchronizationItemSide(Guid id, string name, TKey key, TSideItem sideItem, string sideItemName)
        {
            Id = id;
            Name = name;
            Key = key;
            SideItem = sideItem;
            SideItemName = sideItemName;
        }
        public Guid Id { get; }
        public string Name { get; }
        public TKey Key { get; }
        object ISynchronizationItemSide.Key { get { return Key; } }
        public TSideItem SideItem { get; }
        object ISynchronizationItemSide.SideItem { get { return SideItem; } }
        public string SideItemName { get; }
        public IEnumerable<ISynchronizationProperty> Properties { get; set; } = new List<ISynchronizationProperty>();
        public ICollection<ISynchronizationItem> SubItems { get; set; } = new List<ISynchronizationItem>();
    }
}
