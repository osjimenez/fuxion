using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal class Item<TMasterItem> : IItem
    {
        public Item(TMasterItem masterItem, string masterName)
        {
            MasterItem = masterItem;
            MasterName = masterName;
        }
        public Guid Id { get; } = Guid.NewGuid();
        public string MasterName { get; }
        public TMasterItem MasterItem { get; }
        object IItem.MasterItem { get { return MasterItem; } }
        public IEnumerable<IItemSide> Sides { get; set; } = new List<IItemSide>();
    }
}
