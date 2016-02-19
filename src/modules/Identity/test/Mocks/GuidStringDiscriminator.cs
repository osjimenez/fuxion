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
            TypeId = typeId;
        }
        public Guid Id { get; private set; }
        object IDiscriminator.Id { get { return Id; } }
        public string Name { get; private set; }
        public IEnumerable<GuidStringDiscriminator> Inclusions { get; set; }
        public IEnumerable<GuidStringDiscriminator> Exclusions { get; set; }
        public string TypeId { get; private set; }
        object IDiscriminator.TypeId { get { return TypeId; } }
        public string TypeName { get { return TypeId; } }

        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return Inclusions; } }

        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return Exclusions; } }

        IEnumerable<IDiscriminator<Guid, string>> IInclusive<IDiscriminator<Guid, string>>.Inclusions { get { return Inclusions; } }

        IEnumerable<IDiscriminator<Guid, string>> IExclusive<IDiscriminator<Guid, string>>.Exclusions { get { return Exclusions; } }
    }
}
