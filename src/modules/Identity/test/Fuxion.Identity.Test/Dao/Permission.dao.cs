using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Dao
{
    [Table(nameof(PermissionDao))]
    public class PermissionDao : BaseDao, IPermission
    {
        public RolDao Rol { get; set; }
        public string Function { get; set; }
        public IList<ScopeDao> Scopes { get; set; } = new List<ScopeDao>();
        public bool Value { get; set; }
        public override string Name { get { return $"{Rol?.Name} can{(Value?"":"'t")} {Function} with {Scopes.Count} scopes."; } }


        IFunction IPermission.Function { get { return Functions.GetById(Function); } }
        IEnumerable<IScope> IPermission.Scopes { get { return Scopes; } }
    }
}
