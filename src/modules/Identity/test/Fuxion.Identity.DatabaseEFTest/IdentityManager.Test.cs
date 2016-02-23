using Fuxion.Identity;
using Fuxion.Identity.Test.Entity;
using Fuxion.Identity.Test.Helpers;
using Fuxion.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Fuxion.Identity.Functions;
using static Fuxion.Identity.Test.StaticContext;
using Xunit.Abstractions;
using Fuxion.Factories;

namespace Fuxion.Identity.DatabaseEFTest
{
    public class IdentityFactory : IFactory
    {
        IdentityManager _IdentityManager;
        public object Create(Type type)
        {
            if (type == typeof(IdentityManager))
            {
                if (_IdentityManager == null)
                    _IdentityManager = new IdentityManager(new PasswordProvider(), new IdentityDatabaseEFRepository());
                return _IdentityManager;
            }
            throw new NotImplementedException();
        }
        public IEnumerable<object> GetAllInstances(Type type)
        {
            throw new NotImplementedException();
        }
    }
    public class IdentityManagerTest
    {
        private readonly ITestOutputHelper output;
        public IdentityManagerTest(ITestOutputHelper output)
        {
            this.output = output;
            Factory.AddToPipe(new IdentityFactory());
            TypeDiscriminator.KnownTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith("Fuxion"))
                .SelectMany(a => a.DefinedTypes).ToArray();
            var rep = new IdentityDatabaseEFRepository();
            rep.ClearData();
            rep.Identity.AddRange(Identities);
            rep.Order.AddRange(SellOrders);
            rep.SaveChanges();
        }
        //IdentityManager _IdentityManager;
        IdentityManager IM { get { return Factory.Create<IdentityManager>(); } }
        [Fact]
        public void Login()
        {
            var u = Locations.USA;
            // Check when null values
            Assert.False(IM.Login(null, null));
            Assert.False(IM.Login("root", null));
            Assert.False(IM.Login(null, "root"));
            // Check when empty values
            Assert.False(IM.Login("", ""));
            Assert.False(IM.Login("root", ""));
            Assert.False(IM.Login("", "root"));
            // Check when wrong values
            Assert.False(IM.Login("wrong", "root"));
            Assert.False(IM.Login("root", "wrong"));
            // Check when success
            Assert.True(IM.Login("root", "root"));
        }
        [Fact]
        public void CheckFunctionAssigned()
        {
            // Login
            if (!IM.IsAuthenticated)
                Assert.True(IM.Login("root", "root"));
            // Check if can create & delete objects of type Order
            Assert.True(
                IM.Current
                    .Can(Create, Delete)
                    .OfType<Order>());
            // Check if can create & delete objects of types Order AND Invoice
            Assert.True(
                IM.Current
                    .Can(Create, Delete)
                    .OfAllTypes<Order, Invoice>());
            return;
        }
        [Fact]
        public void Predicate()
        {
            var rep = new IdentityDatabaseEFRepository();
            IM.Login("ca_sell", "ca_sell");
            Printer.PrintAction = message => output.WriteLine(message);
            var res = rep.Order.WhereCan(Read);
            Assert.NotNull(res);
            Assert.True(res.Count() == 2);
        }
    }
}
