using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal interface IItemSideRunner
    {
        string Name { get; }
        object Key { get; }
        object SideItem { get; }
        string SideItemName { get; }
        string SideItemTag { get; }
        ISideRunner Side { get; set; }
        IEnumerable<IPropertyRunner> Properties { get; }
        ICollection<IItemRunner> SubItems { get; set; }
    }
}
