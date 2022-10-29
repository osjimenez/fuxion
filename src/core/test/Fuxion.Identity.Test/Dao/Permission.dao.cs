using System.ComponentModel.DataAnnotations.Schema;

namespace Fuxion.Identity.Test.Dao;

[Table(nameof(PermissionDao))]
public class PermissionDao : BaseDao, IPermission
{
	public PermissionDao(string id, string name, RolDao rol, string function) : base(id, name)
	{
		Rol      = rol;
		Function = function;
	}
	public          RolDao          Rol        { get; set; }
	public          string          Function   { get; set; }
	public          IList<ScopeDao> Scopes     { get; set; } = new List<ScopeDao>();
	public override string          Name       => $"{Rol?.Name} can{(Value ? "" : "'t")} {Function} with {Scopes.Count} scopes.";
	public          bool            Value      { get; set; }
	IFunction IPermission.          Function   => Functions.GetById(Function);
	IEnumerable<IScope> IPermission.Scopes     => Scopes;
	IRol IPermission.               Rol        => Rol;
	public override string          ToString() => this.ToOneLineString();
}