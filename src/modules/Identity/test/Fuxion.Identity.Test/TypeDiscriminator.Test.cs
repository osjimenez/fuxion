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
    static class TypeDiscriminatorFactoryExtensions
    {
        public static void Reset(this TypeDiscriminatorFactory me)
        {
            me.AllowMoreThanOneTypeByDiscriminator = false;
            me.AllowVirtualTypeDiscriminators = false;
            me.ClearRegistrations();
        }
    }
    public class TypeDiscriminatorTetst
    {
        [Fact(DisplayName = "TypeDiscriminator - Register tree")]
        public void RegisterTree()
        {
            var fac = new TypeDiscriminatorFactory();
            fac.AllowVirtualTypeDiscriminators = true;
            // Register from Base
            fac.RegisterTree<BaseDao>();
            var dis = fac.FromType<DocumentDao>();
            Assert.Equal(2, dis.Inclusions.Count());
            Assert.Equal(1, dis.Exclusions.Count());

            fac.Reset();
            // Register from File
            fac.RegisterTree<FileDao>();
            dis = fac.FromType<DocumentDao>();

            Assert.Equal(2, dis.Inclusions.Count());
            Assert.Equal(1, dis.Exclusions.Count());

            fac.Reset();
            fac.AllowMoreThanOneTypeByDiscriminator = true;
            // Register from generic type BaseDvo<>
            fac.RegisterTree(typeof(BaseDvo<>));
            dis = fac.FromType(typeof(LocationDvo<>));
            Assert.Equal(3, dis.Inclusions.Count());
            Assert.Equal(1, dis.Exclusions.Count());

            fac.Reset();
            fac.AllowMoreThanOneTypeByDiscriminator = false;
            // Register from generic type LocationDvo<>
            fac.RegisterTree(typeof(LocationDvo<>));
            dis = fac.FromType<CityDvo>();
            Assert.Equal(0, dis.Inclusions.Count());
            Assert.Equal(1, dis.Exclusions.Count());

            fac.Reset();
            // Register from Base and check disable state for Skill
            fac.RegisterTree<BaseDao>();
            dis = fac.FromType<FileDao>();
            Assert.Equal(3, dis.Inclusions.Count());
            Assert.Equal(1, dis.Exclusions.Count());
            dis = fac.FromType<BaseDao>();
            Assert.Equal(6, dis.Inclusions.Count());
            Assert.Equal(0, dis.Exclusions.Count());
        }
        [Fact(DisplayName = "TypeDiscriminator - Create")]
        public void Create()
        {
            var fac = new TypeDiscriminatorFactory();

            fac.GetIdFunction = (type, att) => att?.Id ?? type.Name;
            fac.GetNameFunction = (type, att) => att?.Name ?? type.Name.ToUpper();

            fac.RegisterTree<BaseDao>(typeof(BaseDao).Assembly.DefinedTypes.ToArray());
            var dis = fac.FromType<DocumentDao>();
            Assert.Equal(TypeDiscriminatorIds.Document, dis.Id);
            Assert.Equal(TypeDiscriminatorIds.Document, dis.Name);
            Assert.Equal(fac.DiscriminatorTypeId, dis.TypeId);
            Assert.Equal(fac.DiscriminatorTypeName, dis.TypeName);
            Assert.Equal(2, dis.Inclusions.Count());
            Assert.Equal(1, dis.Exclusions.Count());
        }
        [Fact(DisplayName = "TypeDiscriminator - New test")]
        public void NewTest()
        {
            var fac = new TypeDiscriminatorFactory();
            fac.Register(typeof(BaseDao));
            fac.Register(typeof(LocationDao));
            fac.Register(typeof(CityDao));
            fac.Register(typeof(CountryDao));

            var dao = fac.FromType<BaseDao>();

            fac.ClearRegistrations();
            fac.Register(typeof(BaseDao));
            fac.Register(typeof(FileDao));
            fac.Register(typeof(DocumentDao));
            fac.Register(typeof(WordDocumentDao));

            dao = fac.FromType<BaseDao>();
        }
        [Fact(DisplayName = "TypeDiscriminator - Attribute")]
        public void TypeDiscriminatorAttribute()
        {
            var fac = new TypeDiscriminatorFactory();

            fac.Register(typeof(BaseDao));
            fac.Register(typeof(LocationDao));
            fac.Register(typeof(CityDao));
            fac.Register(typeof(CountryDao));

            var dao = fac.FromType<CityDao>();

            fac.ClearRegistrations();
            fac.Register(typeof(BaseDto));
            fac.Register(typeof(LocationDto));
            fac.Register(typeof(CityDto));
            fac.Register(typeof(CountryDto));

            var dto = fac.FromType<CityDto>();

            Assert.True(dao.Id == dto.Id, $"Type discriminators DAO & DTO must have same Id. Values are '{dao.Id}' and '{dto.Id}'");

            fac.ClearRegistrations();
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
        [Fact(DisplayName = "TypeDiscriminator - Not allow two same ids")]
        public void NotAllowTwoSameIds()
        {
            var fac = new TypeDiscriminatorFactory();
            Assert.Throws<Exception>(() =>
            {
                fac.Register(typeof(BaseDao));
                fac.Register(typeof(BaseDto));
            });
        }
        [Fact(DisplayName = "TypeDiscriminator - Allow more than one type by discriminator")]
        public void AllowMoreThanOneTypeByDiscriminator()
        {
            var fac = new TypeDiscriminatorFactory();
            fac.AllowMoreThanOneTypeByDiscriminator = true;

            fac.RegisterTree(typeof(BaseDvo<>), typeof(BaseDvo<>).Assembly.DefinedTypes.ToArray());

            var res = fac.AllFromId(TypeDiscriminatorIds.Person);

            Assert.Equal(1, res.Count());
        }
    }
}
