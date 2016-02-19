using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    partial class Identity : IIdentity<string>
    {
        object IIdentity.Id { get { return UserName; } }
    }
}
