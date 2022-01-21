namespace Fuxion.Identity.Test.Dao;

using System.ComponentModel.DataAnnotations.Schema;

[Table(nameof(PermissionDao))]
public class PermissionDao : BaseDao, IPermission
{
	public PermissionDao(string id, string name, RolDao rol, string function) : base(id, name)
	{
		Rol = rol;
		Function = function;
	}

	public RolDao Rol { get; set; }
	public string Function { get; set; }
	public IList<ScopeDao> Scopes { get; set; } = new List<ScopeDao>();
	public bool Value { get; set; }
	public override string Name => $"{Rol?.Name} can{(Value ? "" : "'t")} {Function} with {Scopes.Count} scopes.";

	public override string ToString() => this.ToOneLineString();
	IFunction IPermission.Function => Functions.GetById(Function);
	IEnumerable<IScope> IPermission.Scopes => Scopes;
	IRol IPermission.Rol => Rol;
}