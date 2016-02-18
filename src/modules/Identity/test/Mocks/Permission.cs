using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Mocks
{
    class Permission : IPermission
    {
        public Permission(bool value, IFunction function, params IScope[] scopes)
        {
            Value = value;
            //Rol = rol;
            Function = function;
            Scopes = scopes;
        }
        //public IRol Rol { get; set; }
        public IFunction Function { get; set; }
        public IEnumerable<IScope> Scopes { get; set; }
        public bool Value { get; set; }
    }
}
