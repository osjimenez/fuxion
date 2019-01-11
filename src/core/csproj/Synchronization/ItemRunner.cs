using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal class ItemRunner<TMasterItem> : IItemRunner
    {
        public ItemRunner(ISideRunner masterRunner, TMasterItem masterItem, string masterItemName, string masterItemTag)
        {
            MasterRunner = masterRunner;
            MasterItem = masterItem;
            MasterItemName = masterItemName;
            MasterItemTag = masterItemTag;
        }
        public Guid Id { get; } = Guid.NewGuid();
        public string MasterItemName { get; }
        public string MasterItemTag { get; }
        public TMasterItem MasterItem { get; }
        object IItemRunner.MasterItem { get { return MasterItem; } }
        public ISideRunner MasterRunner { get; }
        public IEnumerable<IItemSideRunner> SideRunners { get; set; } = new List<IItemSideRunner>();
    }
}
