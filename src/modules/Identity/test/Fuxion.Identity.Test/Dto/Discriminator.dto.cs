using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Dto
{
    public abstract class DiscriminatorDto : BaseDto, IDiscriminator<string, string>
    {
        public override string ToString() { return this.ToOneLineString(); }

        object IDiscriminator.Id { get { return Id; } }

        object IDiscriminator.TypeId { get { return GetTypeId(); } }
        protected abstract string GetTypeId();

        string IDiscriminator.TypeName { get { return GetTypeName(); } }
        protected abstract string GetTypeName();

        protected virtual IEnumerable<DiscriminatorDto> GetExclusions() => Enumerable.Empty<DiscriminatorDto>();
        IEnumerable<IDiscriminator> IExclusive<IDiscriminator>.Exclusions { get { return GetExclusions(); } }

        protected virtual IEnumerable<DiscriminatorDto> GetInclusions() => Enumerable.Empty<DiscriminatorDto>();
        IEnumerable<IDiscriminator> IInclusive<IDiscriminator>.Inclusions { get { return GetInclusions(); } }

        string IDiscriminator<string, string>.TypeId { get { return GetTypeId(); } }

        IEnumerable<IDiscriminator<string, string>> IInclusive<IDiscriminator<string, string>>.Inclusions { get { return GetInclusions(); } }

        IEnumerable<IDiscriminator<string, string>> IExclusive<IDiscriminator<string, string>>.Exclusions { get { return GetExclusions(); } }
    }
    [Discriminator("LOC")]
    [TypeDiscriminated(TypeDiscriminatorIds.Location)]
    public abstract partial class LocationDto : DiscriminatorDto
    {
        protected sealed override string GetTypeId() => "LOC";
        protected sealed override string GetTypeName() => "Location";
        protected sealed override IEnumerable<DiscriminatorDto> GetExclusions() => GetLocationExclusions();
        protected abstract IEnumerable<LocationDto> GetLocationExclusions();
        protected sealed override IEnumerable<DiscriminatorDto> GetInclusions() => GetLocationInclusions();
        protected abstract IEnumerable<LocationDto> GetLocationInclusions();
    }
    [TypeDiscriminated(TypeDiscriminatorIds.State)]
    public class StateDao : LocationDto
    {
        public IList<CityDto> Cities { get; set; }
        public CountryDto Country { get; set; }
        protected override IEnumerable<LocationDto> GetLocationExclusions() => new[] { Country };
        protected override IEnumerable<LocationDto> GetLocationInclusions() => Cities.Cast<LocationDto>().ToList();
    }
    [TypeDiscriminated(TypeDiscriminatorIds.Country)]
    public class CountryDto : LocationDto
    {
        public IList<StateDao> States { get; set; }

        protected override IEnumerable<LocationDto> GetLocationExclusions() { return new LocationDto[] { }; }
        protected override IEnumerable<LocationDto> GetLocationInclusions() { return States.Cast<LocationDto>().ToList(); }
    }
    [TypeDiscriminated(TypeDiscriminatorIds.City)]
    public class CityDto : LocationDto
    {
        public StateDao State { get; set; }

        protected override IEnumerable<LocationDto> GetLocationExclusions() { return new[] { State }; }

        protected override IEnumerable<LocationDto> GetLocationInclusions() { return new LocationDto[] { }; }
    }
    [Discriminator("CAT")]
    public class CategoryDto : DiscriminatorDto
    {
        public CategoryDto Parent { get; set; }
        public IEnumerable<CategoryDto> Children { get; set; }

        protected sealed override string GetTypeId() => "CAT";
        protected sealed override string GetTypeName() => nameof(CategoryDto);

        protected override IEnumerable<DiscriminatorDto> GetInclusions() => Children;
        protected override IEnumerable<DiscriminatorDto> GetExclusions() => new[] { Parent };
    }
    public class TagDto : DiscriminatorDto
    {
        protected override string GetTypeId() => "TAG";
        protected override string GetTypeName() => nameof(TagDto);

    }
}
