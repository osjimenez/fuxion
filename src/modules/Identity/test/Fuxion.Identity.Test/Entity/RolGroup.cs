using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    public class RolGroup
    {
        public string RolId { get; set; }
        public Rol Rol { get; set; }
        public string GroupId { get; set; }
        public Group Group { get; set; }
    }
}
