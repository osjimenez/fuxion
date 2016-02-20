using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Permission))]
    public partial class Permission : Base
    {
        public Rol Rol { get; set; }
        public IFunction Function { get; set; }
        public IList<Scope> Scopes { get; set; }
        public bool Value { get; set; }
        public override string ToString()
        {
            return this.ToOneLineString();
        }
    }
}
