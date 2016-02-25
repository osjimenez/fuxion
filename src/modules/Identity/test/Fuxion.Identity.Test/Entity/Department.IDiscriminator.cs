using System.Collections.Generic;
using System.Linq;
namespace Fuxion.Identity.Test.Entity
{
    [Discriminator(TypeId)]
    public partial class Department : IDiscriminator<string, string>
    {
        const string TypeId = "DEP";

        object IDiscriminator.Id { get { return Id; } }

        string IDiscriminator<string,string>.TypeId { get { return TypeId; } }
        protected override object GetTypeId() { return TypeId; }

        string IDiscriminator.TypeName { get { return GetTypeName(); } }
        protected override string GetTypeName() { return nameof(Department); }

        protected override IEnumerable<Discriminator> GetInclusions() { return Children; }
        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions { get { return Children; } }
        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return Children; } }

        protected override IEnumerable<Discriminator> GetExclusions() { return new[] { Parent }; }
        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return new[] { Parent }; } }
        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return new[] { Parent }; } }
    }
}
