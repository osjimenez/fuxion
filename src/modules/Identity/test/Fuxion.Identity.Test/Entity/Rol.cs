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
        [NotMapped]
        public List<Group> Groups
        {
            get { return RolGroups?.Select(rg => rg.Group).ToList(); }
            set { RolGroups = value.Select(g => new RolGroup { Rol = this, RolId = Id, Group = g, GroupId = g.Id }).ToList(); }
        }
        public ICollection<RolGroup> RolGroups { get; set; }
        public List<Permission> Permissions { get; set; }

    }
}
