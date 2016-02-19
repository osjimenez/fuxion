using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.DatabaseTest.Entity
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public partial class Rol : Base
    {
        public string Name { get; set; }
        public IEnumerable<Group> Groups { get; set; }
        public IEnumerable<Permission> Permissions { get; set; }

    }
}
