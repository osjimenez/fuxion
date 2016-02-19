using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Mocks
{
    class Identity : IIdentity<string>
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public IEnumerable<IPermission> Permissions { get; set; }
        public IEnumerable<IGroup> Groups { get; set; }

        object IIdentity.Id { get { return Id; } }
    }
}
