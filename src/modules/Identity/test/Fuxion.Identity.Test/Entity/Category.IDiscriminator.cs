using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    public partial class Category
    {
        protected override IEnumerable<Discriminator> GetInclusions() { return null; }
        protected override IEnumerable<Discriminator> GetExclusions() { return null; }
    }
}
