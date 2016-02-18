using Fuxion.Identity.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test
{
    [TestClass]
    public class TypeDiscriminatorTetst
    {
        [TestMethod]
        public void InitialTest()
        {
            TypeDiscriminator.GetIdFunction = type => type.Name;
            TypeDiscriminator.GetNameFunction = type => type.Name.ToUpper();
            var dis = TypeDiscriminator.Create<Entity>(AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.DefinedTypes).ToArray());
            Debug.WriteLine("");
        }
    }
}
