using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Mocks
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    class GuidFunction : IFunction<Guid>
    {
        public GuidFunction(Guid id, string name) { Id = id; Name = name; }
        public Guid Id { get; private set; }
        object IFunction.Id { get { return Id; } }
        public string Name { get; private set; }
    }
}
