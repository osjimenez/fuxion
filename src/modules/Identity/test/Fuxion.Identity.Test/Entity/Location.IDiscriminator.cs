using System.Collections.Generic;
namespace Fuxion.Identity.Test.Entity
{
    [Discriminator(TypeId)]
    public abstract partial class Location : IDiscriminator<string, string>
    {
        const string TypeId = "LOC";
        protected override object GetTypeId() { return TypeId; }
        object IDiscriminator.Id { get { return Id; } }

        string IDiscriminator<string,string>.TypeId { get { return "LOC"; } }
        object IDiscriminator.TypeId { get { return ((IDiscriminator<string, string>)this).TypeId; } }

        string IDiscriminator.TypeName { get { return GetTypeName(); } }
        protected override string GetTypeName() { return nameof(Location); }

        protected abstract IList<Location> GetInclusions();
        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions { get { return GetInclusions(); } }
        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return GetInclusions(); } }

        protected abstract IList<Location> GetExclusions();
        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return GetExclusions(); } }
        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return GetExclusions(); } }
    }
}
