using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    partial class Group : IGroup
    {
        IEnumerable<IGroup> IRol.Groups { get { return Groups; } }
        IEnumerable<IPermission> IRol.Permissions { get { return Permissions; } }
    }
}
