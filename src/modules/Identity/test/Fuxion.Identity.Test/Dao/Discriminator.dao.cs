using Fuxion.Identity.Test.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Dao
{
    [Table(nameof(DiscriminatorDao))]
    public abstract class DiscriminatorDao : BaseDao, IDiscriminator<string, string>
    {
        public override string ToString() { return this.ToOneLineString(); }

        object IDiscriminator.Id { get { return Id; } }

        object IDiscriminator.TypeId { get { return GetTypeId(); } }
        protected abstract string GetTypeId();

        string IDiscriminator.TypeName { get { return GetTypeName(); } }
        protected abstract string GetTypeName();

        protected virtual IEnumerable<DiscriminatorDao> GetExclusions() => Enumerable.Empty<DiscriminatorDao>();
        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return GetExclusions(); } }

        protected virtual IEnumerable<DiscriminatorDao> GetInclusions() => Enumerable.Empty<DiscriminatorDao>();
        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return GetInclusions(); } }

        string IDiscriminator<string, string>.TypeId { get { return GetTypeId(); } }

        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions { get { return GetInclusions(); } }

        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return GetExclusions(); } }
    }
    [Discriminator("LOC")]
    [TypeDiscriminated(TypeDiscriminatorIds.Location)]
    public abstract partial class LocationDao : DiscriminatorDao
    {
        protected sealed override string GetTypeId() => "LOC";
        protected sealed override string GetTypeName() => nameof(LocationDao);
        protected sealed override IEnumerable<DiscriminatorDao> GetExclusions() => GetLocationExclusions();
        protected abstract IEnumerable<LocationDao> GetLocationExclusions();
        protected sealed override IEnumerable<DiscriminatorDao> GetInclusions() => GetLocationInclusions();
        protected abstract IEnumerable<LocationDao> GetLocationInclusions();
    }
    [Table(nameof(StateDao))]
    [TypeDiscriminated(TypeDiscriminatorIds.State)]
    public class StateDao : LocationDao
    {
        public IList<CityDao> Cities { get; set; }
        public CountryDao Country { get; set; }
        protected override IEnumerable<LocationDao> GetLocationExclusions() => Country != null ? new[] { Country } : null;
        protected override IEnumerable<LocationDao> GetLocationInclusions() => Cities.Cast<LocationDao>().ToList();
    }
    [Table(nameof(CountryDao))]
    [TypeDiscriminated(TypeDiscriminatorIds.Country)]
    public class CountryDao : LocationDao
    {
        public IList<StateDao> States { get; set; }

        protected override IEnumerable<LocationDao> GetLocationExclusions() { return new LocationDao[] { }; }
        protected override IEnumerable<LocationDao> GetLocationInclusions() { return States.Cast<LocationDao>().ToList(); }
    }
    [Table(nameof(CityDao))]
    [TypeDiscriminated(TypeDiscriminatorIds.City)]
    public class CityDao : LocationDao
    {
        public StateDao State { get; set; }

        protected override IEnumerable<LocationDao> GetLocationExclusions() => State != null ? new[] { State } : null;

        protected override IEnumerable<LocationDao> GetLocationInclusions() { return new LocationDao[] { }; }
    }
    [Discriminator("CAT ")]
    [Table(nameof(CategoryDao))]
    public class CategoryDao : DiscriminatorDao
    {
        public CategoryDao Parent { get; set; }
        public IEnumerable<CategoryDao> Children { get; set; }

        protected sealed override string GetTypeId() => "CAT";
        protected sealed override string GetTypeName() => nameof(CategoryDao);

        protected override IEnumerable<DiscriminatorDao> GetInclusions() => Children;
        protected override IEnumerable<DiscriminatorDao> GetExclusions() => Parent != null ? new[] { Parent } : null;
    }
    [Table("TAG")]
    public class TagDao : DiscriminatorDao
    {
        protected override string GetTypeId() => "TAG";
        protected override string GetTypeName() => nameof(TagDao);

    }
}
