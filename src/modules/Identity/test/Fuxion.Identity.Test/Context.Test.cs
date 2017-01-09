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

namespace Fuxion.Identity.Test
{
    public class ContextTest
    {
        public ContextTest(ITestOutputHelper output)
        {
            // Printer
            Printer.PrintAction = m => output.WriteLine(m);
            Container c = new Container();

            // TypeDiscriminators
            c.RegisterSingleton(new TypeDiscriminatorFactory().Transform(fac =>
            {
                fac.RegisterTree<BaseDao>(typeof(BaseDao).Assembly.DefinedTypes.ToArray());
                return fac;
            }));
            // IdentityManager
            c.Register<IPasswordProvider, PasswordProvider>();
            c.RegisterSingleton<ICurrentUserNameProvider>(new GenericCurrentUserNameProvider(() => Context.Rol.Identity.Root.UserName));
            c.RegisterSingleton<IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>>(new IdentityMemoryTestRepository());
            c.Register<IdentityManager>();
            //Factory.AddInjector(new InstanceInjector<IdentityManager>(new IdentityManager()));

            Factory.AddInjector(new SimpleInjectorFactoryInjector(c));
        }
        [Fact]
        public void CreateCalifornia()
        {
            var c = Context.Discriminator.Location.State.California;
            Debug.WriteLine("");

        }
        [Fact]
        public void CheckAdmin()
        {
            var r = Context.Rol;
            Context.RunConfigurationActions();
            Assert.True(Context.Rol.Identity.Root.Can(Create).Type<AlbumDao>());
        }
        [Fact]
        public void AuthorizeToAdmin()
        {
            var r = Context.Rol;
            Context.RunConfigurationActions();
            var res = Context.Rol.Identity.GetAll().AuthorizedTo(Create);
            Printer.Print("Results:");
            Printer.Ident(() =>
            {
                foreach (var ide in res)
                    Printer.Print($"- {ide.Name}");
            });
            Assert.True(res.Count() == 3);
        }
    }
    //public class AlbumList : List<Album>
    //{
    //    public AlbumList()
    //    {
    //        AddRange(new[] { Album_1 });
    //    }
    //    public const string ALBUM_1 = nameof(ALBUM_1);
    //    public Album Album_1 = new Album
    //    {
    //        Id = ALBUM_1,
    //        Songs = new[] { Songs.Song_1 }
    //    };
    //}
}
