using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public interface ISide
    {
        bool IsMaster { get; set; }
        string Name { get; set; }
        string SingularItemTypeName { get; set; }
        string PluralItemTypeName { get; set; }
        bool ItemTypeIsMale { get; set; }
        bool AllowDelete { get; }
        bool AllowInsert { get; }
        bool AllowUpdate { get; }
    }
}
