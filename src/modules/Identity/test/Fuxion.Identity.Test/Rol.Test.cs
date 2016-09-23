using Fuxion.Identity.Test.Entity;
using Fuxion.Identity.Test.Entity;
using Fuxion.Identity.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using static Fuxion.Identity.Functions;
using static Fuxion.Identity.Test.Context;
namespace Fuxion.Identity.Test
{
    [TestClass]
    public class RolTest : BaseTestClass
    {
        public static void Throws<T>(Action action, string message) where T : Exception
        {
            try { action(); }
            catch (T _)
            {
                Debug.WriteLine("");
                return;
            }
            Assert.Fail(message);
        }
        [TestMethod]
        public void CheckFunctionAssigned()
        {
            Assert.Inconclusive("Pending refactor code");

            //var LocationType = Guid.NewGuid();
            //var USA = Guid.NewGuid();
            //var USAPath = new Guid[] { };
            //var California = Guid.NewGuid();
            //var CaliforniaPath = new[] { USA };
            //var SanFrancisco = Guid.NewGuid();
            //var SanFranciscoPath = new[] { USA, California };

            //var Department = Guid.NewGuid();
            //var Sales = Guid.NewGuid();
            //var TI = Guid.NewGuid();
            //var Financial = Guid.NewGuid();

            //var SanFranciscoDis = new GuidDiscriminator(SanFrancisco, "SanFrancisco", LocationType, "LocationType");
            //var CaliforniaDis = new GuidDiscriminator(California, "California", LocationType, "LocationType");
            //var USADis = new GuidDiscriminator(USA, "USA", LocationType, "LocationType");
            //SanFranciscoDis.Exclusions = new[] { CaliforniaDis };
            //CaliforniaDis.Inclusions = new[] { SanFranciscoDis };
            //CaliforniaDis.Exclusions = new[] { USADis };


            //new Rol
            //{
            //    Id = "oka",
            //    Permissions = new[] {
            //        new Permission {
            //            Value =true,
            //            Function = Read.Id.ToString(),
            //            Scopes =new[]{
            //                new Scope {
            //                    Discriminator = Locations.SanFrancisco,
            //                    Propagation = ScopePropagation.ToMe }
            //            }
            //        },
            //        new Permission {
            //            Value =false,
            //            Function =Edit.Id.ToString(),
            //            Scopes =new[] {
            //                new Scope {
            //                    Discriminator = Locations.California,
            //                    Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions }
            //            }
            //        }
            //    }.ToList()
            //}.CheckFunctionAssigned(Read, new[] { SanFranciscoDis }, (m, _) => Debug.WriteLine(m));

            //Assert.IsFalse(new Rol
            //{
            //    Id = "oka",
            //    Permissions = new[] {
            //        new Permission {
            //            Value = true,
            //            Function = Edit.Id.ToString(),
            //            Scopes = new[] {
            //                new Scope {
            //                    Discriminator = Locations.SanFrancisco,
            //                    Propagation = ScopePropagation.ToMe } } },
            //        new Permission {
            //            Value = false,
            //            Function = Edit.Id.ToString(),
            //            Scopes = new[] {
            //                new Scope {
            //                    Discriminator = Locations.California,
            //                    Propagation = ScopePropagation.ToMe | ScopePropagation.ToInclusions } } }
            //    }.ToList()
            //}.IsFunctionAssigned(Edit, new[] { SanFranciscoDis }, (m, _) => Debug.WriteLine(m))
            //    , "Tengo:\r\n" +
            //        $" - Concedido el permiso para {nameof(Edit)} en {nameof(SanFrancisco)}\r\n" +
            //        $" - Denegado el permiso para {nameof(Edit)} en {nameof(California)} y sus hijos\r\n" +
            //        $"¿Debería poder {nameof(Edit)} en {nameof(SanFrancisco)}?\r\n" +
            //        " No");
        }
    }
}
