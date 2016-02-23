using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    public abstract partial class Discriminator : IDiscriminator
    {
        object IDiscriminator.Id { get { return Id; } }

        object IDiscriminator.TypeId { get { return GetTypeId(); } }
        protected abstract object GetTypeId();

        string IDiscriminator.TypeName { get { return GetTypeName(); } }
        protected abstract string GetTypeName();

        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return Exclusions; } }

        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return Inclusions; } }
    }
}
