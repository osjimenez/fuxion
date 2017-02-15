using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    [DebuggerDisplay("{" + nameof(Item) + "}")]
    internal class LoadedItem
    {
        public object Item { get; set; }
        public ICollection<ISideRunner> Sides { get; set; }
    }
}
