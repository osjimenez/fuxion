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
        public GuidStringDiscriminator(Guid id, string name, IEnumerable<Guid> inclusions, IEnumerable<Guid> exclusions, string typeId)
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
        IEnumerable<object> IDiscriminator.Inclusions { get { return ((IDiscriminator<Guid, Guid>)this).Inclusions.Cast<object>(); } }
        public IEnumerable<Guid> Inclusions { get; set; }
        IEnumerable<object> IDiscriminator.Exclusions { get { return ((IDiscriminator<Guid, Guid>)this).Exclusions.Cast<object>(); } }
        public IEnumerable<Guid> Exclusions { get; set; }
        public string TypeId { get; private set; }
        object IDiscriminator.TypeId { get { return TypeId; } }
        public string TypeName { get { return TypeId; } }
    }
}
