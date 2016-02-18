using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Mocks
{
    class Scope : IScope
    {
        public Scope(IDiscriminator discriminator, ScopePropagation propagation)
        {
            Discriminator = discriminator;
            Propagation = propagation;
        }
        public IDiscriminator Discriminator { get; set; }
        public ScopePropagation Propagation { get; set; }
    }
}
