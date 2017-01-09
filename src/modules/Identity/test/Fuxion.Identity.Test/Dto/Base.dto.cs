using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Dto
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    [TypeDiscriminated(TypeDiscriminatorIds.Base)]
    public class BaseDto
    {
        public string Id { get; set; }
        public virtual string Name { get; set; }
    }
}
