using Fuxion.Identity.Test.Entity;
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
using static Fuxion.Identity.Test.StaticContext;
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
                    _IdentityManager = new IdentityManager(new PasswordProvider(), new IdentityMemoryTestRepository()) { Console = (m, _) => Debug.WriteLine(m) };
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
        }
        [TestMethod]
        public void Predicate()
        {
            var rep = new IdentityMemoryTestRepository();
            IM.Login("ca_sell", "ca_sell");
            //var res = SellOrders.WhereCan(Read);
            var res = SellOrders.AsQueryable().AuthorizeTo(Create, Delete);
            //Printer.PrintAction = message => Debug.WriteLine(message);
            //var pred = IM.Current.FilterPredicate<Order>(Read);
            //var ooo = SellOrders.ToList();
            //var res = SellOrders.Where(pred.Compile());
            Assert.IsNotNull(res);
            Assert.IsTrue(res.Count() == 2);
        }
    }
}
