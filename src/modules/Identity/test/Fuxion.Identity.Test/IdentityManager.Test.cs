using Fuxion.Identity.DatabaseTest.Entity;
using Fuxion.Identity.Test.Entity;
using Fuxion.Identity.Test.Helpers;
using Fuxion.Identity.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fuxion.Identity.Functions;
using static Fuxion.Identity.Test.IdentityMemoryRepository;
namespace Fuxion.Identity.Test
{
    [TestClass]
    public class IdentityManagerTest : BaseTestClass
    {
        IdentityManager _IdentityManager;
        IdentityManager IM
        {
            get
            {
                if(_IdentityManager == null)
                    _IdentityManager = new IdentityManager(new PasswordProvider(), new IdentityMemoryRepository()) { Console = (m, _) => Debug.WriteLine(m) };
                return _IdentityManager;
            }
        }
        [TestMethod]
        public void Login()
        {
            var u = Locations.USA;
            // Check when null values
            Assert.IsFalse(IM.Login(null, null));
            Assert.IsFalse(IM.Login("root", null));
            Assert.IsFalse(IM.Login(null, "root"));
            // Check when empty values
            Assert.IsFalse(IM.Login("", ""));
            Assert.IsFalse(IM.Login("root", ""));
            Assert.IsFalse(IM.Login("", "root"));
            // Check when wrong values
            Assert.IsFalse(IM.Login("wrong", "root"));
            Assert.IsFalse(IM.Login("root", "wrong"));
            // Check when success
            Assert.IsTrue(IM.Login("root", "root"));
        }
        [TestMethod]
        public void CheckFunctionAssigned()
        {
            // Login
            if (!IM.IsAuthenticated)
                Assert.IsTrue(IM.Login("root", "root"));
            // Check if can create & delete objects of type Entity
            Assert.IsTrue(
                IM.Current
                    .Can(Create, Delete)
                    .OfType<Order>());
            // Check if can create & delete objects of types Entity AND DerivedEntity
            Assert.IsTrue(
                IM.Current
                    .Can(Create, Delete)
                    .OfAllTypes<Order, Invoice>());
            return;
            var ent = new Order();
            // Check if can edit a concrete Entity
            Assert.IsTrue(
                IM.Current
                    .Can(Edit)
                    .This(ent));

            var ents = new[] { new Order(), new Order() }.AsQueryable();
            // Check if can edit & delete any of these instances
            Assert.IsTrue(
                IM.Current
                    .Can(Edit, Delete)
                    .Any(ents));
            // Check if can read all of these instances
            Assert.IsTrue(
                IM.Current
                    .Can(Read)
                    .All(ents));

            // Filter with queryable
            var filtered = IM.Current
                    .Filter(ents)
                    .For(Read);
            filtered = IM.Current
                    .Filter(ents)
                    .ForAll(Create, Delete);

            // Filter with extension method
            filtered = ents.WhereCan(Read, Create); // Can do all functions
            filtered = ents.WhereCanAny(Read, Create); // Can do one of functions



            IM.Login("root", "root");
            //Assert.IsTrue(IdentityManager.Can(Read).In<Entity>());

            //await IdentityManager.CheckFunctionAssignedAsync("root", new 
            //    StringFunctionGraph(), Read, 
            //    TypeDiscriminator.Create<Entity>(AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.DefinedTypes).ToArray()));
        }
    }
}
