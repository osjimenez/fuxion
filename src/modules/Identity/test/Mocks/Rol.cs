using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Mocks
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    class Rol : IRol
    {
        public Rol(string name, IEnumerable<IGroup> groups, params IPermission[] permissions)
        {
            //Id = id;
            Name = name;
            Groups = groups;
            Permissions = permissions;
        }
        //public Guid Id { get; private set; }
        //object IRol.Id { get { return Id; } }
        public string Name { get; private set; }
        public IEnumerable<IGroup> Groups { get; private set; }

        public IEnumerable<IPermission> Permissions { get; set; }
    }
}
