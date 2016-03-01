using Fuxion.Identity;
using Fuxion.Identity.Test.Entity;
using Fuxion.Identity.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Fuxion.Identity.Functions;
using static Fuxion.Identity.Test.Context;
namespace Fuxion.Identity.DatabaseTest
{
    public class IdentityManagerTest
    {
        public IdentityManagerTest()
        {
            TypeDiscriminator.KnownTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith("Fuxion"))
                .SelectMany(a => a.DefinedTypes).ToArray();
            var rep = new IdentityDatabaseRepository();
            // Uncomment EnsureDeleted() call to clear database
            //rep.Database.EnsureDeleted();
            if (rep.Database.EnsureCreated() || !rep.Identity.Any())
            {
                var iii = Identities.ToArray();
                rep.Identity.AddRange(Identities);
                rep.AttachRange(Identities);
                rep.SaveChanges();
            }
        }
        IdentityManager _IdentityManager;
        IdentityManager IM
        {
            get
            {
                if (_IdentityManager == null)
                    _IdentityManager = new IdentityManager(new PasswordProvider(), new IdentityDatabaseRepository()) { Console = (m, _) => Debug.WriteLine(m) };
                return _IdentityManager;
            }
        }
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
            //if (!IM.IsAuthenticated)
            //    Assert.True(IM.Login("root", "root"));
            //// Check if can create & delete objects of type Entity
            //Assert.True(
            //    IM.Current
            //        .Can(Create, Delete)
            //        .OfType<Order>());
            //// Check if can create & delete objects of types Entity AND DerivedEntity
            //Assert.True(
            //    IM.Current
            //        .Can(Create, Delete)
            //        .OfAllTypes<Order, Invoice>());
            return;
        }
        }
}
