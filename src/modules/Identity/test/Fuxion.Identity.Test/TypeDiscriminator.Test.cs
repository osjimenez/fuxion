using Fuxion.ComponentModel;
using Fuxion.Identity.Test.Entity;
using Fuxion.Identity.Test.Helpers;
using Fuxion.Identity.Test.Mocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Identity.Test
{
    public class TypeDiscriminatorTetst
    {
        [Fact]
        public void Create()
        {
            TypeDiscriminator.GetIdFunction = type => type.Name;
            TypeDiscriminator.GetNameFunction = type => type.Name.ToUpper();
            var dis = TypeDiscriminator.Create<Document>();
            Assert.Equal(dis.Id, typeof(Document).Name);
            Assert.Equal(dis.Name, typeof(Document).Name.ToUpper());
            Assert.Equal(dis.TypeId, TypeDiscriminator.DiscriminatorTypeId);
            Assert.Equal(dis.TypeName, TypeDiscriminator.DiscriminatorTypeName);
            Assert.Equal(dis.Inclusions.Count(), 3);
            Assert.Equal(dis.Exclusions.Count(), 2);
        }
        [Fact]
        public void TypeDiscriminatorAttribute()
        {
            TypeDiscriminator.Register(typeof(BaseDao));
            TypeDiscriminator.Register(typeof(LocationDao));
            TypeDiscriminator.Register(typeof(CityDao));
            TypeDiscriminator.Register(typeof(CountryDao));

            var dao = TypeDiscriminator.FromType<CityDao>();

            TypeDiscriminator.ClearAllRegisters();
            TypeDiscriminator.Register(typeof(BaseDto));
            TypeDiscriminator.Register(typeof(LocationDto));
            TypeDiscriminator.Register(typeof(CityDto));
            TypeDiscriminator.Register(typeof(CountryDto));

            var dto = TypeDiscriminator.FromType<CityDto>();

            Assert.True(dao.Id == dto.Id, $"Type discriminators DAO & DTO must have same Id. Values are '{dao.Id}' and '{dto.Id}'");

            TypeDiscriminator.ClearAllRegisters();
            TypeDiscriminator.Register(typeof(BaseDvo<>));
            TypeDiscriminator.Register(typeof(LocationDvo<>));
            TypeDiscriminator.Register(typeof(CityDvo));
            TypeDiscriminator.Register(typeof(CountryDvo));

            var dvo = TypeDiscriminator.FromId("City");
            var dvo2 = TypeDiscriminator.FromId("Location");
            var dvo3 = TypeDiscriminator.FromType(typeof(LocationDvo<>));

            Assert.True(dao.Id == dvo.Id, $"Type discriminators DAO & DVO must have same Id. Values are '{dao.Id}' and '{dvo.Id}'");
        }
        [Fact]
        public void NotAllowTwoSameIds()
        {
            Assert.Throws()

            TypeDiscriminator.Register(typeof(BaseDao));
            TypeDiscriminator.Register(typeof(BaseDto));
        }
    }

    [TypeDiscriminator("Base")]
    public abstract class BaseDao { }
    [TypeDiscriminator("Location")]
    public abstract class LocationDao : BaseDao { }
    [TypeDiscriminator("City")]
    public class CityDao : LocationDao { }
    [TypeDiscriminator("Country")]
    public class CountryDao : LocationDao { }

    [TypeDiscriminator("Base")]
    public abstract class BaseDto { }
    [TypeDiscriminator("Location")]
    public abstract class LocationDto : BaseDto { }
    [TypeDiscriminator("City")]
    public class CityDto : LocationDto { }
    [TypeDiscriminator("Country")]
    public class CountryDto : LocationDto { }

    [TypeDiscriminator("Base")]
    public abstract class BaseDvo<T> : Notifier<BaseDvo<T>>{ }
    [TypeDiscriminator("Location")]
    public abstract class LocationDvo<T> : BaseDvo<LocationDvo<T>>{ }
    [TypeDiscriminator("City")]
    public class CityDvo : LocationDvo<CityDvo> { }
    [TypeDiscriminator("Country")]
    public class CountryDvo : LocationDvo<CountryDvo> { }
}
