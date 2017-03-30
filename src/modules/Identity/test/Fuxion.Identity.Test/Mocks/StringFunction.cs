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
        public StringFunction(string id) { Id = id; Name = id; }

        public string Id { get; private set; }
        object IFunction.Id { get { return Id; } }

        public string Name { get; set; }

        public IEnumerable<IFunction<string>> Inclusions { get; private set; }
        IEnumerable<IFunction> IInclusive<IFunction>.Inclusions { get { return Inclusions; } }

        public IEnumerable<IFunction<string>> Exclusions { get; private set; }
        IEnumerable<IFunction> IExclusive<IFunction>.Exclusions { get { return Inclusions; } }
    }
}
