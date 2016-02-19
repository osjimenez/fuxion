using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Mocks
{
    [Discriminator("DIS")]
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    class DiscriminatorEntity : IDiscriminator<string, string>
    {
        
        public string Id { get; set; }
        object IDiscriminator.Id { get { return Id; } }

        public string Name { get; set; }

        public string TypeId { get; set; }
        object IDiscriminator.TypeId { get { return TypeId; } }

        public string TypeName { get; set; }

        public IEnumerable<DiscriminatorEntity> Inclusions { get { throw new NotImplementedException(); } }
        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return Inclusions; } }
        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions { get { return Inclusions; } }

        public IEnumerable<DiscriminatorEntity> Exclusions { get { throw new NotImplementedException(); } }
        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return Exclusions; } }
        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return Exclusions; } }
    }
}
