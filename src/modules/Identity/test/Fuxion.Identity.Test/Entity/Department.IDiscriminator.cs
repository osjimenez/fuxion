using System.Collections.Generic;
namespace Fuxion.Identity.Test.Entity
{
    [Discriminator(TypeId)]
    public partial class Department : IDiscriminator<string, string>
    {
        const string TypeId = "DEP";
        protected override object GetTypeId() { return TypeId; }

        object IDiscriminator.Id { get { return Id; } }

        string IDiscriminator<string,string>.TypeId { get { return TypeId; } }
        //object IDiscriminator.TypeId { get { return TypeId; } }

        string IDiscriminator.TypeName { get { return TypeId; } }

        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions { get { return Children; } }
        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return Children; } }

        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return new[] { Parent }; } }
        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return new[] { Parent }; } }
    }
}
