using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal interface IProperty
    {
        string PropertyName { get; }
        object MasterValue { get; }
        object SideValue { get; }
    }
}
