using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    public partial class Permission : IPermission
    {
        IFunction IPermission.Function { get { return Functions.GetById(Function); } }

        IEnumerable<IScope> IPermission.Scopes { get { return Scopes; } }
    }
}
