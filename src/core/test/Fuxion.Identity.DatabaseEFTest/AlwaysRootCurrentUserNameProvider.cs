using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.DatabaseEFTest
{
    public class AlwaysRootCurrentUserNameProvider : ICurrentUserNameProvider
    {
        public string GetCurrentUserName()
        {
            return "root";
        }
    }
}
