using Fuxion.Factories;
using Fuxion.Identity.Test.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Helpers
{
    public static class Bootstrapper
    {
        static bool initialized = false;
        public static void Initialize() {
            if (initialized) return;

            //TypeDiscriminator.KnownTypes = AppDomain.CurrentDomain.GetAssemblies()
            //                                .SelectMany(a => a.DefinedTypes).ToArray();
            //Factory.AddInjector(new InstanceInjector<TypeDiscriminatorFactory>(new TypeDiscriminatorFactory()));
            //Factory.Get<TypeDiscriminatorFactory>().RegisterTree<Base>(typeof(Base).Assembly.DefinedTypes.ToArray());

            initialized = true;
        }
    }
}
