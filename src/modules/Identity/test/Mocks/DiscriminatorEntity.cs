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
        public IEnumerable<DiscriminatorEntity> Exclusions { get { throw new NotImplementedException(); } }
        public string Id { get { throw new NotImplementedException(); } }
        public IEnumerable<DiscriminatorEntity> Inclusions { get { throw new NotImplementedException(); } }
        public string Name { get { throw new NotImplementedException(); } }
        public string TypeId { get { throw new NotImplementedException(); } }
        object IDiscriminator.TypeId { get { return TypeId; } }
        public string TypeName { get { throw new NotImplementedException(); } }

        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return Exclusions; } }

        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return Exclusions; } }

        //IEnumerable<object> IExclusive<object>.Exclusions { get { return Exclusions; } }

        object IDiscriminator.Id { get { return Id; } }

        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return Inclusions; } }

        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
    }
}
