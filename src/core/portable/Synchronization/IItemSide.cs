using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal interface IItemSide
    {
        //Guid Id { get; }
        string Name { get; }
        object Key { get; }
        object SideItem { get; }
        string SideItemName { get; }
        ISideRunner Side { get; set; }
        IEnumerable<IProperty> Properties { get; }
        ICollection<IItem> SubItems { get; set; }
    }
}
