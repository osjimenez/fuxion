using Fuxion.Factories;
using Fuxion.Identity.Test.Dao;
using SimpleInjector;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using Xunit;
using Xunit.Abstractions;
using static Fuxion.Identity.Functions;
using Fuxion.Identity.Test.Helpers;
using Fuxion.Repositories;
using Fuxion.Test;
using Fuxion.Identity.Test.Repositories;
using Fuxion.Identity.Test.Mocks;

namespace Fuxion.Identity.Test
{
    public class ContextTest : BaseTest
    {
        public ContextTest(ITestOutputHelper output) : base(output)
        {
            Container c = new Container();

            // TypeDiscriminators
            c.RegisterSingleton(new TypeDiscriminatorFactory().Transform(fac =>
            {
                fac.RegisterTree<BaseDao>(typeof(BaseDao).Assembly.DefinedTypes.ToArray());
                return fac;
            }));
            // IdentityManager
            c.Register<IPasswordProvider, PasswordProviderMock>();
            c.RegisterSingleton<ICurrentUserNameProvider>(new GenericCurrentUserNameProvider(() => Context.Rols.Identity.Root.UserName));
            c.RegisterSingleton<IKeyValueRepository<string, IIdentity>>(new IdentityMemoryTestRepository());
            c.Register<IdentityManager>();
            //Factory.AddInjector(new InstanceInjector<IdentityManager>(new IdentityManager()));

            Factory.AddInjector(new SimpleInjectorFactoryInjector(c));
        }
        //[Fact]
        //public void CreateCalifornia()
        //{
        //    var c = Context.Discriminator.Location.State.California;
        //    Debug.WriteLine("");

        //}
        //[Fact]
        //public void CheckAdmin()
        //{
        //    var r = Context.Rol;
        //    Context.RunConfigurationActions();
        //    Assert.True(Context.Rol.Identity.Root.Can(Create).Type<AlbumDao>());
        //}
        //[Fact]
        //public void AuthorizeToAdmin()
        //{
        //    var r = Context.Rol;
        //    Context.RunConfigurationActions();
        //    var res = Context.Rol.Identity.GetAll().AuthorizedTo(Create);
        //    Printer.Print("Results:");
        //    Printer.Ident(() =>
        //    {
        //        foreach (var ide in res)
        //            Printer.Print($"- {ide.Name}");
        //    });
        //    Assert.True(res.Count() == 3);
        //}
    }
}
