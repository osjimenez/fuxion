using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Discriminator("LOC")]
    public abstract partial class Location : Discriminator
    {
        protected sealed override string GetTypeId() => "LOC";
        protected sealed override string GetTypeName() => nameof(Location);
        protected sealed override IEnumerable<Discriminator> GetExclusions() => GetLocationExclusions();
        protected abstract IEnumerable<Location> GetLocationExclusions();
        protected sealed override IEnumerable<Discriminator> GetInclusions() => GetLocationInclusions();
        protected abstract IEnumerable<Location> GetLocationInclusions();
    }
}
