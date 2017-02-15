using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public class ItemSidePreview
    {
        internal ItemSidePreview() { }
        internal ItemSidePreview(Guid id) { Id = id; }
        public Guid Id { get; set; }

        public bool SideItemExist { get; set; }
        public string SideName { get; set; }
        public string Key { get; set; }
        public string SideItemName { get; set; }
        public SynchronizationAction Action { get; set; }

        public ICollection<PropertyPreview> Properties { get; set; } = new List<PropertyPreview>();
        public ICollection<ItemRelationPreview> Relations { get; set; } = new List<ItemRelationPreview>();
    }
}
