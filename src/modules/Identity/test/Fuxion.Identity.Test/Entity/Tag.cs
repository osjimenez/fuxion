using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Tag))]
    public partial class Tag : Discriminator
    {
        //protected override IList<Discriminator> Exclusions { get { return null; } }
        //protected override IList<Discriminator> Inclusions { get { return null; } }
    }
}
