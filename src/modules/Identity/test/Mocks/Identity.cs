using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Mocks
{
    class Identity : IIdentity
    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public IEnumerable<IPermission> Permissions { get; set; }
        public IEnumerable<IGroup> Groups { get; set; }
    }
}
