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
    }
}
