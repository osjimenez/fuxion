using System.Collections.Generic;
namespace Fuxion.Identity.DatabaseTest.Entity
{
    [Discriminator("DEP")]
    public partial class Department : IDiscriminator<string, string>
    {
        object IDiscriminator.Id { get { return Id; } }

        public string TypeId { get { return "DEP"; } }
        object IDiscriminator.TypeId { get { return TypeId; } }

        public string TypeName { get { return TypeId; } }

        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions { get { return Children; } }
        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return Children; } }

        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return new[] { Parent }; } }
        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return new[] { Parent }; } }
    }
}
