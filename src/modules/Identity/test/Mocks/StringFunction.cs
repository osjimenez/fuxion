using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Mocks
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    class StringFunction : IFunction<string>
    {
        public StringFunction(string id) { Id = id; }
        public string Id { get; private set; }
        object IFunction.Id { get { return Id; } }
        public string Name { get { return Id; } }
    }
}
