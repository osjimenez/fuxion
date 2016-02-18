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
        public IEnumerable<string> Exclusions { get { throw new NotImplementedException(); } }
        public string Id { get { throw new NotImplementedException(); } }
        public IEnumerable<string> Inclusions { get { throw new NotImplementedException(); } }
        public string Name { get { throw new NotImplementedException(); } }
        public string TypeId { get { throw new NotImplementedException(); } }
        public string TypeName { get { throw new NotImplementedException(); } }
        IEnumerable<object> IDiscriminator.Exclusions { get { return Exclusions; } }
        object IDiscriminator.Id { get { return Id; } }
        IEnumerable<object> IDiscriminator.Inclusions { get { return Inclusions; } }
        object IDiscriminator.TypeId { get { return TypeId; } }
    }
}
