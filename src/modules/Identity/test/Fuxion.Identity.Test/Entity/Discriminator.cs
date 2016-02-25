using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Discriminator))]
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public abstract partial class Discriminator : Base
    {
        public string Name { get; set; }
        //abstract protected IList<Discriminator> Exclusions { get; }
        //abstract protected IList<Discriminator> Inclusions { get; }

        public override string ToString() { return this.ToOneLineString(); }
    }
}
