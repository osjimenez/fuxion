using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Mocks
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    class StringDiscriminator : IDiscriminator<string, string>
    {
        public StringDiscriminator(string id, IEnumerable<StringDiscriminator> inclusions, IEnumerable<StringDiscriminator> exclusions, string typeId)
        {
            Id = id;
            Inclusions = inclusions;
            Exclusions = exclusions;
            TypeId = typeId;
        }
        public string Id { get; private set; }
        object IDiscriminator.Id { get { return Id; } }
        public string Name { get { return Id; } }
        public IEnumerable<StringDiscriminator> Inclusions { get; set; }
        public IEnumerable<StringDiscriminator> Exclusions { get; set; }
        public string TypeId { get; private set; }
        object IDiscriminator.TypeId { get { return TypeId; } }
        public string TypeName { get { return TypeId; } }

        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return Inclusions; } }

        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return Exclusions; } }

        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions { get { return Inclusions; } }

        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return Exclusions; } }
    }
}
