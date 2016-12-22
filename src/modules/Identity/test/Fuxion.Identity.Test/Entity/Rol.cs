using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Rol))]
    public abstract class Rol : Base, IRol
    {
        public IList<Group> Groups { get; set; } = new List<Group>();
        public IList<Permission> Permissions { get; set; } = new List<Permission>();
        IEnumerable<IGroup> IRol.Groups { get { return Groups; } }
        IEnumerable<IPermission> IRol.Permissions { get { return Permissions; } }
    }
    [Table(nameof(Group))]
    public class Group : Rol, IGroup
    {
        public List<Rol> Rols { get; set; } = new List<Rol>();
        IEnumerable<IGroup> IRol.Groups { get { return Groups; } }
        IEnumerable<IPermission> IRol.Permissions { get { return Permissions; } }

    }
    [Table(nameof(Identity))]
    [TypeDiscriminated(nameof(Identity))]
    public class Identity : Rol, IIdentity<string>
    {
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        object IIdentity.Id { get { return UserName; } }
    }
}
