using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.DatabaseTest.Entity
{
    public partial class Scope
    {
        public Scope(IDiscriminator discriminator, ScopePropagation propagation)
        {
            Discriminator = discriminator;
            Propagation = propagation;
        }
        public override string ToString()
        {
            return this.ToOneLineString();
        }
    }
}
