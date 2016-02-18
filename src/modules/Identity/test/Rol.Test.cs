using Fuxion.Identity.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Fuxion.Identity.GuidFunctionGraph;
namespace Fuxion.Identity.Test
{
    [TestClass]
    public class RolTest
    {
        public static void Throws<T>(Action action, string message) where T : Exception
        {
            try { action(); }
            catch (T _) { return; }
            Assert.Fail(message);
        }
        [TestMethod]
        public void CheckFunctionAssigned()
        {
            var LocationType = Guid.NewGuid();
            var USA = Guid.NewGuid();
            var USAPath = new Guid[] { };
            var California = Guid.NewGuid();
            var CaliforniaPath = new[] { USA };
            var SanFrancisco = Guid.NewGuid();
            var SanFranciscoPath = new[] { USA, California };

            var Department = Guid.NewGuid();
            var Sales = Guid.NewGuid();
            var TI = Guid.NewGuid();
            var Financial = Guid.NewGuid();

            var SanFranciscoDis = new GuidDiscriminator(SanFrancisco, "SanFrancisco", new Guid[] { }, new[] { USA, California }, LocationType, "LocationType");
            var CaliforniaDis = new GuidDiscriminator(California, "California", new[] { SanFrancisco }, new[] { USA }, LocationType, "LocationType");

            new Rol("oka", null,
                new Permission(true, Read, new Scope(SanFranciscoDis, ScopePropagation.ToMe)),
                new Permission(false, Edit,
                    new Scope(CaliforniaDis, ScopePropagation.ToMe | ScopePropagation.ToInclusions))
                ).CheckFunctionAssigned(new GuidFunctionGraph(), Read, new[] { SanFranciscoDis }, null);

            Throws<UnauthorizedAccessException>(() =>
                new Rol("oka", null,
                    new Permission(true, Edit, new Scope(SanFranciscoDis, ScopePropagation.ToMe)),
                    new Permission(false, Edit, new Scope(CaliforniaDis, ScopePropagation.ToMe | ScopePropagation.ToInclusions))
                ).CheckFunctionAssigned(new GuidFunctionGraph(), Edit, new[] { SanFranciscoDis }, null)
                , "Tengo:\r\n" +
                    $" - Concedido el permiso para {nameof(Edit)} en {nameof(SanFrancisco)}\r\n" +
                    $" - Denegado el permiso para {nameof(Edit)} en {nameof(California)} y sus hijos\r\n" +
                    $"¿Debería poder {nameof(Edit)} en {nameof(SanFrancisco)}?\r\n" +
                    " No");
        }
    }
}
