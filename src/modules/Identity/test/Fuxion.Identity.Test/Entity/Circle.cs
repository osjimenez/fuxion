using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Circle))]
    public partial class Circle : Discriminator
    {
        public IList<Circle> Excluded { get; set; }
        public IList<Circle> Included { get; set; }
    }
}
