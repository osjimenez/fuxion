using Fuxion.Factories;
using Fuxion.Identity.Test;
using Fuxion.Identity.Test.Dao;
using Fuxion.Identity.Test.Helpers;
using Fuxion.Identity.Test.Mocks;
using Fuxion.Identity.Test.Repositories;
using Fuxion.Repositories;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.DatabaseEFTest
{
    public static class Scenario
    {
        public const string DATABASE = nameof(DATABASE);
        public const string MEMORY = nameof(MEMORY);
        public static void Load(string key)
        {
            //TypeDiscriminator.KnownTypes = AppDomain.CurrentDomain.GetAssemblies()
            //    .Where(a => a.FullName.StartsWith("Fuxion"))
            //    .SelectMany(a => a.DefinedTypes).ToArray();
            Factory.RemoveAllInjectors();
            if (key == MEMORY)
            {
                if (memoryFactory == null)
                {
                    var con = new Container();
                    con.RegisterSingleton<ICurrentUserNameProvider>(new AlwaysRootCurrentUserNameProvider());
                    con.RegisterSingleton<IPasswordProvider>(new PasswordProviderMock());
                    var rep = new IdentityMemoryTestRepository();
                    con.RegisterSingleton<IKeyValueRepository<string, IIdentity>>(rep);
                    con.RegisterSingleton<IIdentityTestRepository>(rep);
                    con.RegisterSingleton<IdentityManager>();

                    var fac = new TypeDiscriminatorFactory();
                    fac.RegisterTree<BaseDao>(typeof(BaseDao).Assembly.DefinedTypes.ToArray());
                    con.RegisterSingleton(fac);

                    memoryFactory = new SimpleInjectorFactoryInjector(con);
                }
                Factory.AddInjector(memoryFactory);
            }
            else if (key == DATABASE)
            {
                if (databaseFactory == null)
                {
                    var con = new Container();
                    con.RegisterSingleton<ICurrentUserNameProvider>(new AlwaysRootCurrentUserNameProvider());
                    con.RegisterSingleton<IPasswordProvider>(new PasswordProviderMock());
                    var rep = new IdentityDatabaseEFTestRepository();
                    rep.Initialize();
                    con.RegisterSingleton<IKeyValueRepository<string, IIdentity>>(new MemoryCachedKeyValueRepository<string, IIdentity>(rep));
                    con.RegisterSingleton<IIdentityTestRepository>(rep);
                    con.RegisterSingleton<IdentityManager>();

                    var fac = new TypeDiscriminatorFactory();
                    fac.RegisterTree<BaseDao>(typeof(BaseDao).Assembly.DefinedTypes.ToArray());
                    con.RegisterSingleton(fac);

                    databaseFactory = new SimpleInjectorFactoryInjector(con);
                }
                Factory.AddInjector(databaseFactory);
            }
            else throw new NotImplementedException($"El escenario '{key}' no esta soportado");
        }
        static SimpleInjectorFactoryInjector memoryFactory;
        static SimpleInjectorFactoryInjector databaseFactory;
    }
}
