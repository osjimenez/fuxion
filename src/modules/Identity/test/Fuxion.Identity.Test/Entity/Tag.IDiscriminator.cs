using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Discriminator(TypeId)]
    public partial class Tag
    {
        const string TypeId = "TAG";
        protected override object GetTypeId() { return TypeId; }
        protected override string GetTypeName() { return nameof(Tag); }
        protected override IEnumerable<Discriminator> GetInclusions() { return null; }
        protected override IEnumerable<Discriminator> GetExclusions() { return null; }
    }
}
