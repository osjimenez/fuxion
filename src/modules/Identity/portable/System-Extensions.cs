using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace Fuxion.Identity
{
    public static class Extensions
    {
        public static FuxionIdentity FuxionPrincipal(this System.Security.Principal.IPrincipal me)
        {
            return me.Identity as FuxionIdentity;
        }
    }
}
