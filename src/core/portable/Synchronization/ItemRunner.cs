using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal class ItemRunner<TMasterItem> : IItemRunner
    {
        public ItemRunner(TMasterItem masterItem, string masterName)
        {
            MasterItem = masterItem;
            MasterName = masterName;
        }
        public Guid Id { get; } = Guid.NewGuid();
        public string MasterName { get; }
        public TMasterItem MasterItem { get; }
        object IItemRunner.MasterItem { get { return MasterItem; } }
        public IEnumerable<IItemSideRunner> Sides { get; set; } = new List<IItemSideRunner>();
    }
}
