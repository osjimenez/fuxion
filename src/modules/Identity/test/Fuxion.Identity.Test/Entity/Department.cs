using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    partial class Department : Base
    {
        public string Name { get; set; }
        public IEnumerable<Department> Children { get; set; }
        public Department Parent { get; set; }
    }
}
