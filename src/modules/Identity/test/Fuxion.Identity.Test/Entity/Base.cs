using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public abstract class Base
    {
        public string Id { get; set; }
        public virtual string Name { get; set; }
    }
}
