using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    public partial class Rol : IRol
    {
        IEnumerable<IGroup> IRol.Groups { get { return Groups; } }
        IEnumerable<IPermission> IRol.Permissions { get { return Permissions; } }
    }
}
