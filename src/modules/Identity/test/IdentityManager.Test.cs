using Fuxion.Identity.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fuxion.Identity.Functions;
namespace Fuxion.Identity.Test
{
    [TestClass]
    public class IdentityManagerTest
    {
        IdentityManager _IdentityManager;
        public IdentityManager IdentityManager
        {
            get
            {
                if(_IdentityManager == null)
                    _IdentityManager = new IdentityManager(new PasswordProvider(), new IdentityRepository()) { Console = (m, _) => Debug.WriteLine(m) };
                return _IdentityManager;
            }
        }
        [TestMethod]
        public void ValidateCredentials()
        {
            Assert.IsTrue(IdentityManager.ValidateCredentials("root", "root"));
        }
        [TestMethod]
        public void CheckFunctionAssigned()
        {
            // Login
            if (!IdentityManager.IsAuthenticated)
                IdentityManager.ValidateCredentials("root", "root");
            // Check if can create & delete Entities
            Assert.IsTrue(
                IdentityManager.Current
                    .Can(Create, Delete)
                    .In<Entity>());

            var ent = new Entity();
            // Check if can edit a concrete Entity
            Assert.IsTrue(
                IdentityManager.Current
                    .Can(Edit)
                    .This(ent));

            var ents = new[] { new Entity(), new Entity() }.AsQueryable();
            // Check if can edit & delete any of these instances
            Assert.IsTrue(
                IdentityManager.Current
                    .Can(Edit, Delete)
                    .Any(ents));
            // Check if can read all of these instances
            Assert.IsTrue(
                IdentityManager.Current
                    .Can(Read)
                    .All(ents));

            // Filter with queryable
            var filtered = IdentityManager.Current
                    .Filter(ents)
                    .For(Read);
            filtered = IdentityManager.Current
                    .Filter(ents)
                    .ForAll(Create, Delete);

            // Filter with extension method
            filtered = ents.WhereCan(Read, Create); // Can do all functions
            filtered = ents.WhereCanAny(Read, Create); // Can do one of functions



            IdentityManager.ValidateCredentials("root", "root");
            //Assert.IsTrue(IdentityManager.Can(Read).In<Entity>());

            //await IdentityManager.CheckFunctionAssignedAsync("root", new 
            //    StringFunctionGraph(), Read, 
            //    TypeDiscriminator.Create<Entity>(AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.DefinedTypes).ToArray()));
        }
    }
}
