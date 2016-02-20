using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    public partial class Scope : IScope
    {
        IDiscriminator IScope.Discriminator { get { return Discriminator; } }
    }
}
