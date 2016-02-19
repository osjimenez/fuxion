using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    partial class Permission
    {
        public IFunction Function { get; set; }
        public IEnumerable<IScope> Scopes { get; set; }
        public bool Value { get; set; }
        public override string ToString()
        {
            return this.ToOneLineString();
        }
    }
}
