using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Discriminator))]
    public partial class Discriminator : Base
    {
        public string Name { get; set; }
        public IList<Discriminator> Exclusions { get; set; }
        public IList<Discriminator> Inclusions { get; set; }
    }
}
