using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal interface IItem
    {
        Guid Id { get; }
        object MasterItem { get; }
        string MasterName { get; }
        IEnumerable<IItemSide> Sides { get; }
    }
}
