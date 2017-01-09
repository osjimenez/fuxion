using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Dvo
{
    public abstract class DiscriminatorDvo<TDiscriminator> : BaseDvo<TDiscriminator>, IDiscriminator<string, string>
        where TDiscriminator : DiscriminatorDvo<TDiscriminator>
    {
        public override string ToString() { return this.ToOneLineString(); }

        object IDiscriminator.Id { get { return Id; } }

        object IDiscriminator.TypeId { get { return GetTypeId(); } }
        string IDiscriminator<string, string>.TypeId { get { return GetTypeId(); } }
        protected abstract string GetTypeId();

        string IDiscriminator.TypeName { get { return GetTypeName(); } }
        protected abstract string GetTypeName();


        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions { get { return GetInclusions(); } }
        protected virtual IEnumerable<IDiscriminator<string,string>> GetInclusions() => Enumerable.Empty<DiscriminatorDvo<TDiscriminator>>();
        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return GetInclusions(); } }

        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return GetExclusions(); } }
        protected virtual IEnumerable<IDiscriminator<string,string>> GetExclusions() => Enumerable.Empty<DiscriminatorDvo<TDiscriminator>>();
        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return GetExclusions(); } }
    }
    [Discriminator("LOC")]
    [TypeDiscriminated(TypeDiscriminatorIds.Location)]
    public abstract partial class LocationDvo<TLocation> : DiscriminatorDvo<TLocation>
        where TLocation : LocationDvo<TLocation>
    {
        protected sealed override string GetTypeId() => "LOC";
        protected sealed override string GetTypeName() => "Location";
    }
    [TypeDiscriminated(TypeDiscriminatorIds.Country)]
    public class CountryDvo : LocationDvo<CountryDvo>
    {
        public IList<StateDvo> States { get; set; }
        protected override IEnumerable<IDiscriminator<string, string>> GetInclusions() => States;
    }
    [TypeDiscriminated(TypeDiscriminatorIds.State)]
    public class StateDvo : LocationDvo<StateDvo>
    {
        public IList<CityDvo> Cities { get; set; }
        public CountryDvo Country { get; set; }
        protected override IEnumerable<IDiscriminator<string, string>> GetExclusions() => new[] { Country };
        protected override IEnumerable<IDiscriminator<string, string>> GetInclusions() => Cities;
    }
    [TypeDiscriminated(TypeDiscriminatorIds.City)]
    public class CityDvo : LocationDvo<CityDvo>
    {
        public StateDvo State { get; set; }
        protected override IEnumerable<IDiscriminator<string, string>> GetExclusions() => new[] { State };
    }
    [Discriminator("CAT")]
    public class CategoryDvo : DiscriminatorDvo<CategoryDvo>
    {
        public CategoryDvo Parent { get; set; }
        public IEnumerable<CategoryDvo> Children { get; set; }

        protected sealed override string GetTypeId() => "CAT";
        protected sealed override string GetTypeName() => nameof(CategoryDvo);

        protected override IEnumerable<IDiscriminator<string, string>> GetExclusions() => new[] { Parent };
        protected override IEnumerable<IDiscriminator<string, string>> GetInclusions() => Children;
    }
    public class TagDvo : DiscriminatorDvo<TagDvo>
    {
        protected override string GetTypeId() => "TAG";
        protected override string GetTypeName() => nameof(TagDvo);

    }
}
