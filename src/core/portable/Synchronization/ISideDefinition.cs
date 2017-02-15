using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public interface ISideDefinition
    {
        bool IsMaster { get; set; }
        string Name { get; set; }
        //string SingularItemTypeName { get; set; }
        //string PluralItemTypeName { get; set; }
    }
}
