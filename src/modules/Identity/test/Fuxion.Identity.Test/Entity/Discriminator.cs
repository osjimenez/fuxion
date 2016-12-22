using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Entity
{
    [Table(nameof(Discriminator))]
    public abstract class Discriminator : Base, IDiscriminator<string, string>
    {
        public override string ToString() { return this.ToOneLineString(); }

        object IDiscriminator.Id { get { return Id; } }

        object IDiscriminator.TypeId { get { return GetTypeId(); } }
        protected abstract string GetTypeId();

        string IDiscriminator.TypeName { get { return GetTypeName(); } }
        protected abstract string GetTypeName();

        protected virtual IEnumerable<Discriminator> GetExclusions() => Enumerable.Empty<Discriminator>();
        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return GetExclusions(); } }

        protected virtual IEnumerable<Discriminator> GetInclusions() => Enumerable.Empty<Discriminator>();
        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return GetInclusions(); } }

        string IDiscriminator<string, string>.TypeId { get { return GetTypeId(); } }

        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions { get { return GetInclusions(); } }

        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return GetExclusions(); } }
    }
    [Discriminator("LOC")]
    public abstract partial class Location : Discriminator
    {
        protected sealed override string GetTypeId() => "LOC";
        protected sealed override string GetTypeName() => nameof(Location);
        protected sealed override IEnumerable<Discriminator> GetExclusions() => GetLocationExclusions();
        protected abstract IEnumerable<Location> GetLocationExclusions();
        protected sealed override IEnumerable<Discriminator> GetInclusions() => GetLocationInclusions();
        protected abstract IEnumerable<Location> GetLocationInclusions();
    }
    [Table(nameof(State))]
    public class State : Location
    {
        public IList<City> Cities { get; set; }
        public Country Country { get; set; }
        protected override IEnumerable<Location> GetLocationExclusions() => new[] { Country };
        protected override IEnumerable<Location> GetLocationInclusions() => Cities.Cast<Location>().ToList();
    }
    [Table(nameof(Country))]
    public class Country : Location
    {
        public IList<State> States { get; set; }

        protected override IEnumerable<Location> GetLocationExclusions() { return new Location[] { }; }
        protected override IEnumerable<Location> GetLocationInclusions() { return States.Cast<Location>().ToList(); }
    }
    [Table(nameof(City))]
    public class City : Location
    {
        public State State { get; set; }

        protected override IEnumerable<Location> GetLocationExclusions() { return new[] { State }; }

        protected override IEnumerable<Location> GetLocationInclusions() { return new Location[] { }; }
    }
    [Discriminator("CAT")]
    [Table(nameof(Category))]
    public class Category : Discriminator
    {
        public Category Parent { get; set; }
        public IEnumerable<Category> Children { get; set; }

        protected sealed override string GetTypeId() => "CAT";
        protected sealed override string GetTypeName() => nameof(Category);

        protected override IEnumerable<Discriminator> GetInclusions() => Children;
        protected override IEnumerable<Discriminator> GetExclusions() => new[] { Parent };
    }
    [Table("TAG")]
    public class Tag : Discriminator
    {
        protected override string GetTypeId() => "TAG";
        protected override string GetTypeName() => nameof(Tag);

    }
}
