using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal interface IComparatorResultInternal //: IComparatorResult
    {
        object Key { get; }
        object MasterItem { get; }
        object SideItem { get; }
        ICollection<IPropertyRunner> Properties { get; set; }
        ICollection<ISideRunner> SubSides { get; set; }
    }
}
