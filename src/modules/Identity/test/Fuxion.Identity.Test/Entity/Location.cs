using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    public abstract partial class Location : Discriminator
    {
        //protected override IList<Discriminator> Exclusions { get { return GetExclusions().Cast<Discriminator>().ToList(); } }
        //protected override IList<Discriminator> Inclusions { get { return GetInclusions().Cast<Discriminator>().ToList(); } }
    }
}
