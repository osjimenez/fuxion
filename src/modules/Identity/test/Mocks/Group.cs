using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Mocks
{
    class Group : IGroup
    {
        public string Name { get; set; }
        public IEnumerable<IPermission> Permissions { get; set; }
        public IEnumerable<IGroup> Groups { get; set; }
        //public IEnumerable<IRol> Rols { get; set; }
    }
}
