using Fuxion.Identity.Test.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Dao
{
    [Table(nameof(RolDao))]
    [TypeDiscriminated(TypeDiscriminatorIds.Rol)]
    public abstract class RolDao : BaseDao, IRol
    {
        public IList<GroupDao> Groups { get; set; } = new List<GroupDao>();
        public IList<PermissionDao> Permissions { get; set; } = new List<PermissionDao>();
        IEnumerable<IGroup> IRol.Groups { get { return Groups; } }
        IEnumerable<IPermission> IRol.Permissions { get { return Permissions; } }

        [DiscriminatedBy(typeof(CategoryDao))]
        public string CategoryId { get; set; }
        public CategoryDao Category { get; set; }
    }
    [Table(nameof(GroupDao))]
    [TypeDiscriminated(TypeDiscriminatorIds.Group)]
    public class GroupDao : RolDao, IGroup
    {
        public List<RolDao> Rols { get; set; } = new List<RolDao>();
        IEnumerable<IGroup> IRol.Groups { get { return Groups; } }
        IEnumerable<IPermission> IRol.Permissions { get { return Permissions; } }

    }
    [Table(nameof(IdentityDao))]
    [TypeDiscriminated(TypeDiscriminatorIds.Identity)]
    public class IdentityDao : RolDao, IIdentity<string>
    {
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        object IIdentity.Id { get { return UserName; } }
    }
}
