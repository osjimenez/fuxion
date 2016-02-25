using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    public partial class Circle : IDiscriminator<string, string>
    {
        const string TypeId = "CIR";

        string IDiscriminator<string, string>.TypeId { get { return TypeId; } }

        protected override object GetTypeId() { return TypeId; }
        protected override string GetTypeName() { return nameof(Circle); }

        protected override IEnumerable<Discriminator> GetInclusions() { return Inclusions; }
        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions { get { return Inclusions; } }

        protected override IEnumerable<Discriminator> GetExclusions() { return Exclusions; }
        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return Exclusions; } }


    }
}
