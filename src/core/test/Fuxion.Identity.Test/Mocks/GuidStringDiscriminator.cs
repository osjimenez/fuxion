using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Mocks
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    class GuidStringDiscriminator : IDiscriminator<Guid, string>
    {
        public GuidStringDiscriminator(Guid id, string name, IEnumerable<GuidStringDiscriminator> inclusions, IEnumerable<GuidStringDiscriminator> exclusions, string typeId)
        {
            Id = id;
            Name = name;
            Inclusions = inclusions;
            Exclusions = exclusions;
            TypeKey = typeId;
        }

        public Guid Id { get; private set; }
        object IDiscriminator.Id { get { return Id; } }

        public string Name { get; private set; }

        public string TypeKey { get; private set; }
        object IDiscriminator.TypeKey { get { return TypeKey; } }

        public string TypeName { get { return TypeKey; } }

        public IEnumerable<GuidStringDiscriminator> Inclusions { get; set; }
        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return Inclusions; } }
        IEnumerable<IDiscriminator<Guid, string>> IInclusive<IDiscriminator<Guid, string>>.Inclusions { get { return Inclusions; } }

        public IEnumerable<GuidStringDiscriminator> Exclusions { get; set; }
        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return Exclusions; } }
        IEnumerable<IDiscriminator<Guid, string>> IExclusive<IDiscriminator<Guid, string>>.Exclusions { get { return Exclusions; } }
    }
}
