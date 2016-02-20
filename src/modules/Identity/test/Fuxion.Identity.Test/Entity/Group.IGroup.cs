using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    public partial class Group : IGroup
    {
        IEnumerable<IGroup> IRol.Groups { get { return RolGroups.Select(rg=>rg.Group); } }
        IEnumerable<IPermission> IRol.Permissions { get { return Permissions; } }
    }
}
