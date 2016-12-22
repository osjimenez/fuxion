using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Permission))]
    public class Permission : Base, IPermission
    {
        public Rol Rol { get; set; }
        public string Function { get; set; }
        public IList<Scope> Scopes { get; set; } = new List<Scope>();
        public bool Value { get; set; }
        public override string Name { get { return $"{Rol?.Name} can{(Value?"":"'t")} {Function} with {Scopes.Count} scopes."; } }
        public override string ToString()
        {
            return this.ToOneLineString();
        }


        IFunction IPermission.Function { get { return Functions.GetById(Function); } }
        IEnumerable<IScope> IPermission.Scopes { get { return Scopes; } }
    }
}
