using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Department))]
    public partial class Department : Discriminator
    {
        public IList<Department> Children { get; set; }
        public Department Parent { get; set; }
    }
}
