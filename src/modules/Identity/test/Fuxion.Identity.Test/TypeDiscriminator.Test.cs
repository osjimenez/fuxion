using Fuxion.ComponentModel;
using Fuxion.Identity.Test.Dao;
using Fuxion.Identity.Test.Dto;
using Fuxion.Identity.Test.Dvo;
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
            // Register from Base
            fac.RegisterTree<Dao.BaseDao>(typeof(Dao.BaseDao).Assembly.DefinedTypes.ToArray());
            var dis = fac.FromType<DocumentDao>();
            Assert.Equal(3, dis.Inclusions.Count());
            Assert.Equal(1, dis.Exclusions.Count());

            fac.ClearAllRegisters();
            // Register from File
            fac.RegisterTree<FileDao>(typeof(FileDao).Assembly.DefinedTypes.ToArray());
            dis = fac.FromType<DocumentDao>();

            Assert.Equal(3, dis.Inclusions.Count());
            Assert.Equal(1, dis.Exclusions.Count());

            fac.ClearAllRegisters();
            fac.AllowMoreThanOneTypeByDiscriminator = true;
            // Register from generic type BaseDvo<>
            fac.RegisterTree(typeof(BaseDvo<>), typeof(BaseDvo<>).Assembly.DefinedTypes.ToArray());
            dis = fac.FromType(typeof(LocationDvo<>));
            Assert.Equal(3, dis.Inclusions.Count());
            Assert.Equal(1, dis.Exclusions.Count());

            fac.ClearAllRegisters();
            fac.AllowMoreThanOneTypeByDiscriminator = false;
            // Register from generic type LicationDvo<>
            fac.RegisterTree(typeof(LocationDvo<>), typeof(LocationDvo<>).Assembly.DefinedTypes.ToArray());
            dis = fac.FromType<CityDvo>();

            Assert.Equal(0, dis.Inclusions.Count());
            Assert.Equal(1, dis.Exclusions.Count());

            fac.ClearAllRegisters();
            // Register from Base and check disable state for Skill
            fac.RegisterTree<Dao.BaseDao>(typeof(Dao.BaseDao).Assembly.DefinedTypes.ToArray());
            dis = fac.FromType<FileDao>();

            Assert.Equal(3, dis.Inclusions.Count());
            Assert.Equal(1, dis.Exclusions.Count());

            dis = fac.FromType<Dao.BaseDao>();

            Assert.Equal(6, dis.Inclusions.Count());
            Assert.Equal(0, dis.Exclusions.Count());
        }
        [Fact]
        public void Create()
        {
            var fac = new TypeDiscriminatorFactory();

            fac.GetIdFunction = type => type.Name;
            fac.GetNameFunction = type => type.Name.ToUpper();

            fac.RegisterTree<Dao.BaseDao>(typeof(Dao.BaseDao).Assembly.DefinedTypes.ToArray());
            var dis = fac.FromType<DocumentDao>();
            Assert.Equal(dis.Id, typeof(DocumentDao).Name);
            Assert.Equal(dis.Name, typeof(DocumentDao).Name.ToUpper());
            Assert.Equal(dis.TypeId, fac.DiscriminatorTypeId);
            Assert.Equal(dis.TypeName, fac.DiscriminatorTypeName);
            Assert.Equal(3, dis.Inclusions.Count());
            Assert.Equal(1, dis.Exclusions.Count());
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

            var dvo  = fac.FromId(TypeDiscriminatorIds.City);
            var dvo2 = fac.FromId(TypeDiscriminatorIds.Location);
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
        [Fact]
        public void AllowTwoSameIds()
        {
            var fac = new TypeDiscriminatorFactory();
            fac.AllowMoreThanOneTypeByDiscriminator = true;

            fac.RegisterTree(typeof(BaseDvo<>), typeof(BaseDvo<>).Assembly.DefinedTypes.ToArray());

            var res = fac.AllFromId(TypeDiscriminatorIds.Person);

            Assert.Equal(2, res.Count());
        }
    }
}
