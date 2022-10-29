using System.ComponentModel.DataAnnotations.Schema;

namespace Fuxion.Identity.Test.Dao;

[Table(nameof(RolDao))]
[TypeDiscriminated(Helpers.TypeDiscriminatorIds.Rol)]
public abstract class RolDao : BaseDao, IRol
{
	public RolDao(string id, string name) : base(id, name) { }
	public IList<GroupDao>      Groups      { get; set; } = new List<GroupDao>();
	public IList<PermissionDao> Permissions { get; set; } = new List<PermissionDao>();
	[DiscriminatedBy(typeof(CategoryDao))]
	public string? CategoryId { get;                 set; }
	public CategoryDao?           Category    { get; set; }
	IEnumerable<IGroup> IRol.     Groups      => Groups;
	IEnumerable<IPermission> IRol.Permissions => Permissions;
}

[Table(nameof(GroupDao))]
[TypeDiscriminated(Helpers.TypeDiscriminatorIds.Group)]
public class GroupDao : RolDao, IGroup
{
	public GroupDao(string id, string name) : base(id, name) { }
	public List<RolDao>           Rols        { get; set; } = new();
	IEnumerable<IGroup> IRol.     Groups      => Groups;
	IEnumerable<IPermission> IRol.Permissions => Permissions;
}

[Table(nameof(IdentityDao))]
[TypeDiscriminated(Helpers.TypeDiscriminatorIds.Identity)]
public class IdentityDao : RolDao, IIdentity<string>
{
#nullable disable
	public IdentityDao(string id, string name) : base(id, name) { }
#nullable enable
	public IdentityDao(string id, string name, string userName, byte[] passwordHash, byte[] passwordSalt) : this(id, name)
	{
		UserName     = userName;
		PasswordHash = passwordHash;
		PasswordSalt = passwordSalt;
	}
	public string    UserName     { get; set; }
	public byte[]    PasswordHash { get; set; }
	public byte[]    PasswordSalt { get; set; }
	object IIdentity.Id           => UserName;
}