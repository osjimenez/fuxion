using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.DatabaseTest.Entity
{
    public partial class Scope : IScope
    {
        public IDiscriminator Discriminator { get; set; }
        public ScopePropagation Propagation { get; set; }
    }
}
