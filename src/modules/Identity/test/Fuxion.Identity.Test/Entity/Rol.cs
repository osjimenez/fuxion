using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    [Table(nameof(Rol))]
    public partial class Rol : Base
    {
        public string Name { get; set; }
        public IList<Group> Groups { get; set; }
        public IList<Permission> Permissions { get; set; }

    }
}
