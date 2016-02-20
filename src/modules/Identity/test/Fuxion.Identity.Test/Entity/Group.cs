using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Group))]
    public partial class Group : Rol
    {
        public ICollection<RolGroup> GroupsRol{ get; set; }
        [NotMapped]
        public List<Rol> Rols {
            get { return GroupsRol?.Select(rg => rg.Rol).ToList(); }
            set { GroupsRol = value.Select(r => new RolGroup { Rol = r, RolId = r.Id, Group = this, GroupId = Id }).ToList(); }
        }
    }
}
