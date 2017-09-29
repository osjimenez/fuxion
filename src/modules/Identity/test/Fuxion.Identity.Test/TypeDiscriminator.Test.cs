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
using Xunit.Abstractions;

namespace Fuxion.Identity.Test
{
    static class TypeDiscriminatorFactoryExtensions
    {
        public static void Reset(this TypeDiscriminatorFactory me)
        {
            me.AllowMoreThanOneTypeByDiscriminator = false;
            me.ClearRegistrations();
        }
    }
    public class TypeDiscriminatorTetst : Fuxion.Test.BaseTest
    {
        public TypeDiscriminatorTetst(ITestOutputHelper output) : base(output) { this.output = output; }
        ITestOutputHelper output;
        [Fact(DisplayName = "TypeDiscriminator - Equality")]
        public void Equality()
        {
            var fac = new TypeDiscriminatorFactory();
            fac.AllowMoreThanOneTypeByDiscriminator = true;
            // Register from Base
            fac.RegisterTree<BaseDao>();
            fac.RegisterTree(typeof(BaseDvo<>));

            var d1 = fac.FromType<FileDao>();
            var d2 = fac.FromType(typeof(FileDvo<>));

            Assert.True(d1 == d2);
            Assert.Equal(d1, d2);
            Assert.Same(d1, d2);

            var d1c = new[] { d1 };
            Assert.True(d1c.Contains(d2));
        }
        [Fact(DisplayName = "TypeDiscriminator - Register - Twice")]
        public void RegisterTwice()
        {
            var fac = new TypeDiscriminatorFactory();
            // Register from Base
            fac.Register<BaseDao>();
            Assert.Throws(typeof(Exception), () =>
            {
                fac.Register<BaseDao>();
            });
        }
        [Fact(DisplayName = "TypeDiscriminator - Register - Two trees in parallel")]
        public void RegisterTwoTrees()
        {
            var fac = new TypeDiscriminatorFactory();
            fac.AllowMoreThanOneTypeByDiscriminator = true;
            // Register from Base
            fac.RegisterTree<BaseDao>();
            fac.RegisterTree(typeof(BaseDvo<>));

            fac.FromType<BaseDao>();
        }
        [Fact(DisplayName = "TypeDiscriminator - Register - All tree")]
        public void RegisterAllTree()
        {
            var fac = new TypeDiscriminatorFactory();
            // Register from Base
            fac.RegisterTree<BaseDao>();
            var dis = fac.FromType<DocumentDao>();
            Assert.Equal(2, dis.Inclusions.Count());
            Assert.Equal(1, dis.Exclusions.Count());
        }
        [Fact(DisplayName = "TypeDiscriminator - Register - File tree")]
        public void RegisterFileTree()
        {
            var fac = new TypeDiscriminatorFactory();
            // Register from File
            fac.RegisterTree<FileDao>();
            TypeDiscriminator dis = null;
            Assert.Throws(typeof(TypeDiscriminatorRegistrationValidationException), () =>
            {
                dis = fac.FromType<DocumentDao>();
            });
            fac.Reset();
            fac.RegisterTree<FileDao>();
            fac.Register<BaseDao>();
            dis = fac.FromType<DocumentDao>();
            Assert.Equal(2, dis.Inclusions.Count());
            Assert.Equal(1, dis.Exclusions.Count());
        }
        [Fact(DisplayName = "TypeDiscriminator - Register - Generic")]
        public void RegisterGeneric()
        {
            var fac = new TypeDiscriminatorFactory();
            fac.RegisterTree(typeof(FileDvo<>));
            fac.Register(typeof(BaseDvo<>));
            var dis = fac.FromType(typeof(DocumentDvo<>));
            Assert.Equal(2, dis.Inclusions.Count());
            Assert.Equal(1, dis.Exclusions.Count());
        }
        [Fact(DisplayName = "TypeDiscriminator - Register - Only File")]
        public void RegisterOnlyttttFile()
        {
            var fac = new TypeDiscriminatorFactory();
            fac.AllowMoreThanOneTypeByDiscriminator = true;
            // Register from generic type BaseDvo<>
            fac.RegisterTree(typeof(BaseDvo<>));
            var dis = fac.FromType(typeof(LocationDvo<>));
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
        }
        [Fact(DisplayName = "TypeDiscriminator - Register - Disable")]
        public void RegisterDisabgleState()
        {
            var fac = new TypeDiscriminatorFactory();
            fac.RegisterTree<BaseDao>();
            var dis = fac.FromType<FileDao>();
            Assert.Equal(3, dis.Inclusions.Count());
            Assert.Equal(1, dis.Exclusions.Count());
            dis = fac.FromType<BaseDao>();
            Assert.Equal(7, dis.Inclusions.Count());
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
            Assert.Equal(TypeDiscriminator.TypeDiscriminatorId, dis.TypeId);
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

            fac.Reset();
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
            fac.Reset();
            fac.Register(typeof(BaseDto));
            fac.Register(typeof(LocationDto));
            fac.Register(typeof(CityDto));
            fac.Register(typeof(CountryDto));

            var dto = fac.FromType<CityDto>();

            Assert.True(dao.Id == dto.Id, $"Type discriminators DAO & DTO must have same Id. Values are '{dao.Id}' and '{dto.Id}'");

            fac.ClearRegistrations();
            fac.Register(typeof(BaseDvo<>));
            fac.Register(typeof(LocationDvo<>));
            fac.Register(typeof(CityDvo));
            fac.Register(typeof(CountryDvo));

            var dvo  = fac.FromId(TypeDiscriminatorIds.City);
            var dvo2 = fac.FromId(TypeDiscriminatorIds.Location);
            var dvo3 = fac.FromType(typeof(LocationDvo<>));

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
