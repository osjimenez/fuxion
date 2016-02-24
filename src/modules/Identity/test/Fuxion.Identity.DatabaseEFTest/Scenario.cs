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
    public class Scenario
    {
        public const string Database = nameof(Database);
        public const string Memory = nameof(Memory);
        public static void Load(string key)
        {
            TypeDiscriminator.KnownTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith("Fuxion"))
                .SelectMany(a => a.DefinedTypes).ToArray();
            Factory.ClearPipe();
            var con = new Container();
            con.RegisterSingleton<IPasswordProvider>(new PasswordProvider());
            if (key == Memory)
            {
                var rep = new IdentityMemoryTestRepository();
                con.RegisterSingleton<IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>>(rep);
                con.RegisterSingleton<IIdentityTestRepository>(rep);
            }
            else if (key == Database)
            {
                var rep = new IdentityDatabaseEFTestRepository();
                rep.InitializeData();
                con.RegisterSingleton<IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>>(rep);
                con.RegisterSingleton<IIdentityTestRepository>(rep);
            }
            else throw new NotImplementedException($"El escenario '{key}' no esta soportado");
            con.RegisterSingleton<IdentityManager>();
            var fac = new SimpleInjectorFactory(con);
            Factory.AddToPipe(fac);
        }
    }
}
