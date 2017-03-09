using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal class ItemRunner<TMasterItem> : IItemRunner
    {
        public ItemRunner(ISideRunner masterRunner, TMasterItem masterItem, string masterName)
        {
            MasterRunner = masterRunner;
            MasterItem = masterItem;
            MasterName = masterName;
        }
        public Guid Id { get; } = Guid.NewGuid();
        public string MasterName { get; }
        public TMasterItem MasterItem { get; }
        object IItemRunner.MasterItem { get { return MasterItem; } }
        public ISideRunner MasterRunner { get; }
        public IEnumerable<IItemSideRunner> SideRunners { get; set; } = new List<IItemSideRunner>();
    }
}
