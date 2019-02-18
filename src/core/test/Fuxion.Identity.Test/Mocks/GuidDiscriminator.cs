using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Mocks
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    class GuidDiscriminator : IDiscriminator<Guid, Guid>
    {
        public GuidDiscriminator(Guid id, string name, Guid typeId, string typeName)
        {
            Id = id;
            Name = name;
            TypeKey = typeId;
            TypeName = typeName;
        }
        public Guid Id { get; private set; }
        object IDiscriminator.Id { get { return Id; } }

        public string Name { get; private set; }

        public Guid TypeKey { get; private set; }
        object IDiscriminator.TypeKey { get { return TypeKey; } }

        public string TypeName { get; private set; }

        public IEnumerable<GuidDiscriminator> Inclusions { get; set; }
        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return Inclusions; } }
        IEnumerable<IDiscriminator<Guid, Guid>> IInclusive<IDiscriminator<Guid, Guid>>.Inclusions { get { return Inclusions; } }

        public IEnumerable<GuidDiscriminator> Exclusions { get; set; }
        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return Exclusions; } }
        IEnumerable<IDiscriminator<Guid, Guid>> IExclusive<IDiscriminator<Guid, Guid>>.Exclusions { get { return Exclusions; } }
    }
}
