using Fuxion.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using static System.Security.Claims.ClaimsPrincipal;
using System.Security.Claims;

namespace Fuxion.Identity_universal
{
    public class Class1 : IPrincipalProvider
    {
        public Class1()
        {
            System.Security.Principal.GenericPrincipal.
            ClaimsPrincipalSelector = ()=> { return currentPrincipal; };
        }
        ClaimsPrincipal currentPrincipal;
        public IPrincipal GetPrincipal()
        {
            return Current;
        }
        public void SetPrincipal(IPrincipal principal)
        {
            //currentPrincipal = principal;
        }
    }
}
