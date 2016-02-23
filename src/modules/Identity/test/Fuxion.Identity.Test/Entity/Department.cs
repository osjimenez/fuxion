using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
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

        protected override IList<Discriminator> Exclusions { get { return new[] { Parent }; } }
        protected override IList<Discriminator> Inclusions { get { return Children.Cast<Discriminator>().ToList(); }  }
    }
}
