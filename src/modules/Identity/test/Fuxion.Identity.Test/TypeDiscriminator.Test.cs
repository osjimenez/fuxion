using Fuxion.Identity.Test.Entity;
using Fuxion.Identity.Test.Entity;
using Fuxion.Identity.Test.Helpers;
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
    public class TypeDiscriminatorTetst : BaseTestClass
    {
        [TestMethod]
        public void Create()
        {
            TypeDiscriminator.GetIdFunction = type => type.Name;
            TypeDiscriminator.GetNameFunction = type => type.Name.ToUpper();
            var dis = TypeDiscriminator.Create<Document>();
            Assert.AreEqual(dis.Id, typeof(Document).Name);
            Assert.AreEqual(dis.Name, typeof(Document).Name.ToUpper());
            Assert.AreEqual(dis.TypeId, TypeDiscriminator.DiscriminatorTypeId);
            Assert.AreEqual(dis.TypeName, TypeDiscriminator.DiscriminatorTypeName);
            Assert.AreEqual(dis.Inclusions.Count(), 3);
            Assert.AreEqual(dis.Exclusions.Count(), 2);

        }
    }
}
