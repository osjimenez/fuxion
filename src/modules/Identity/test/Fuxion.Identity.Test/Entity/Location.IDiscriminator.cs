using System.Linq;
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

        protected abstract IEnumerable<Location> GetLocationInclusions();
        protected override IEnumerable<Discriminator> GetInclusions() { return GetLocationInclusions(); }
        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions { get { return GetLocationInclusions(); } }
        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return GetInclusions(); } }

        protected abstract IEnumerable<Location> GetLocationExclusions();
        protected override IEnumerable<Discriminator> GetExclusions() { return GetLocationExclusions(); }
        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return GetLocationExclusions(); } }
        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return GetExclusions(); } }
    }
}
