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
        public void RegisterTree()
        {
            var fac = new TypeDiscriminatorFactory();

            fac.RegisterTree<Base>(typeof(Base).Assembly.DefinedTypes.ToArray());
            var dis = fac.FromType<Document>();
            Assert.Equal(dis.Inclusions.Count(), 3);
            Assert.Equal(dis.Exclusions.Count(), 2);

            fac.ClearAllRegisters();

            fac.RegisterTree<File>(typeof(File).Assembly.DefinedTypes.ToArray());
            dis = fac.FromType<Document>();

            Assert.Equal(dis.Inclusions.Count(), 3);
            Assert.Equal(dis.Exclusions.Count(), 1);

            fac.ClearAllRegisters();

            fac.RegisterTree(typeof(BaseDvo<>), typeof(BaseDvo<>).Assembly.DefinedTypes.ToArray());
            dis = fac.FromType(typeof(LocationDvo<>));
            Assert.Equal(dis.Inclusions.Count(), 2);
            Assert.Equal(dis.Exclusions.Count(), 1);

            fac.ClearAllRegisters();

            fac.RegisterTree(typeof(LocationDvo<>), typeof(LocationDvo<>).Assembly.DefinedTypes.ToArray());
            dis = fac.FromType<CityDvo>();

            Assert.Equal(dis.Inclusions.Count(), 0);
            Assert.Equal(dis.Exclusions.Count(), 1);
        }
        [Fact]
        public void Create()
        {
            var fac = new TypeDiscriminatorFactory();

            fac.GetIdFunction = type => type.Name;
            fac.GetNameFunction = type => type.Name.ToUpper();

            fac.RegisterTree<Base>(typeof(Base).Assembly.DefinedTypes.ToArray());
            var dis = fac.FromType<Document>();
            Assert.Equal(dis.Id, typeof(Document).Name);
            Assert.Equal(dis.Name, typeof(Document).Name.ToUpper());
            Assert.Equal(dis.TypeId, fac.DiscriminatorTypeId);
            Assert.Equal(dis.TypeName, fac.DiscriminatorTypeName);
            Assert.Equal(dis.Inclusions.Count(), 3);
            Assert.Equal(dis.Exclusions.Count(), 2);
        }
        [Fact]
        public void TypeDiscriminatorAttribute()
        {
            var fac = new TypeDiscriminatorFactory();

            fac.Register(typeof(BaseDao));
            fac.Register(typeof(LocationDao));
            fac.Register(typeof(CityDao));
            fac.Register(typeof(CountryDao));

            var dao = fac.FromType<CityDao>();

            fac.ClearAllRegisters();
            fac.Register(typeof(BaseDto));
            fac.Register(typeof(LocationDto));
            fac.Register(typeof(CityDto));
            fac.Register(typeof(CountryDto));

            var dto = fac.FromType<CityDto>();

            Assert.True(dao.Id == dto.Id, $"Type discriminators DAO & DTO must have same Id. Values are '{dao.Id}' and '{dto.Id}'");

            fac.ClearAllRegisters();
            fac.Register(typeof(BaseDvo<>));
            fac.Register(typeof(LocationDvo<>));
            //fac.Register(typeof(LocationDvo<CityDvo>));
            fac.Register(typeof(CityDvo));
            fac.Register(typeof(CountryDvo));

            var dvo  = fac.FromId("City");
            var dvo2 = fac.FromId("Location");
            var dvo3 = fac.FromType(typeof(LocationDvo<>));
            //var dvo3 = fac.FromType(typeof(LocationDvo<CityDvo>));

            Assert.True(dao.Id == dvo.Id, $"Type discriminators DAO & DVO must have same Id. Values are '{dao.Id}' and '{dvo.Id}'");
            Assert.True(dvo2.Id == dvo3.Id, $"Type discriminators DAO & DVO must have same Id. Values are '{dvo2.Id}' and '{dvo3.Id}'");

            Assert.True(dvo2.Inclusions.Contains(dvo), $"Type discriminator 'Location' must include 'City'");
        }
        [Fact]
        public void NotAllowTwoSameIds()
        {
            var fac = new TypeDiscriminatorFactory();
            Assert.Throws<Exception>(() =>
            {
                fac.Register(typeof(BaseDao));
                fac.Register(typeof(BaseDto));
            });
        }
    }

    [TypeDiscriminated("Base")]
    public abstract class BaseDao { }
    [TypeDiscriminated("Location")]
    public abstract class LocationDao : BaseDao { }
    [TypeDiscriminated("City")]
    public class CityDao : LocationDao { }
    [TypeDiscriminated("Country")]
    public class CountryDao : LocationDao { }

    [TypeDiscriminated("Base")]
    public abstract class BaseDto { }
    [TypeDiscriminated("Location")]
    public abstract class LocationDto : BaseDto { }
    [TypeDiscriminated("City")]
    public class CityDto : LocationDto { }
    [TypeDiscriminated("Country")]
    public class CountryDto : LocationDto { }

    [TypeDiscriminated("Base")]
    public abstract class BaseDvo<T> : Notifier<BaseDvo<T>>{ }
    [TypeDiscriminated("Location")]
    public abstract class LocationDvo<T> : BaseDvo<LocationDvo<T>>{ }
    [TypeDiscriminated("City")]
    public class CityDvo : LocationDvo<CityDvo> { }
    [TypeDiscriminated("Country")]
    public class CountryDvo : LocationDvo<CountryDvo> { }
}
