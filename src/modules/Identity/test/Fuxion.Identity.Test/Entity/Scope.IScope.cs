using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    partial class Scope : IScope
    {
        public IDiscriminator Discriminator { get; set; }
        public ScopePropagation Propagation { get; set; }
    }
}
