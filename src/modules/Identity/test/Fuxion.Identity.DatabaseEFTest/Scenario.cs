using Fuxion.Factories;
using Fuxion.Identity.Test;
using Fuxion.Identity.Test.Helpers;
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
            TypeDiscriminator.KnownTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith("Fuxion"))
                .SelectMany(a => a.DefinedTypes).ToArray();
            Factory.ClearPipe();
            if (key == MEMORY)
            {
                if (memoryFactory == null)
                {
                    var con = new Container();
                    con.RegisterSingleton<IPasswordProvider>(new PasswordProvider());
                    var rep = new IdentityMemoryTestRepository();
                    con.RegisterSingleton<IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>>(rep);
                    con.RegisterSingleton<IIdentityTestRepository>(rep);
                    con.RegisterSingleton<IdentityManager>();
                    memoryFactory = new SimpleInjectorFactory(con);
                }
                Factory.AddToPipe(memoryFactory);
            }
            else if (key == DATABASE)
            {
                if (databaseFactory == null)
                {
                    var con = new Container();
                    con.RegisterSingleton<IPasswordProvider>(new PasswordProvider());
                    var rep = new IdentityDatabaseEFTestRepository();
                    rep.Initialize();
                    con.RegisterSingleton<IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>>(new MemoryKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>(rep));
                    con.RegisterSingleton<IIdentityTestRepository>(rep);
                    con.RegisterSingleton<IdentityManager>();
                    databaseFactory = new SimpleInjectorFactory(con);
                }
                Factory.AddToPipe(databaseFactory);
            }
            else throw new NotImplementedException($"El escenario '{key}' no esta soportado");
        }
        static SimpleInjectorFactory memoryFactory;
        static SimpleInjectorFactory databaseFactory;
    }
}
