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
using static Fuxion.Identity.Test.Context;
using Xunit.Abstractions;
using Fuxion.Factories;
using System.Linq.Expressions;
using Xunit.Extensions;
using System.Security.Permissions;
using Fuxion.Identity.Test;
using System.Collections;
using SimpleInjector;
using Fuxion.Repositories;

namespace Fuxion.Identity.DatabaseEFTest
{
    public class IdentityManagerTest
    {
        public IdentityManagerTest(ITestOutputHelper output)
        {
            Printer.PrintAction = m => output.WriteLine(m);
        }
        #region Login
        [Theory]
        [InlineData(new object[] { Scenario.Memory, null, null, false })]
        [InlineData(new object[] { Scenario.Memory, "root", null, false })]
        [InlineData(new object[] { Scenario.Memory, null, "root", false })]
        [InlineData(new object[] { Scenario.Memory, "", "", false })]
        [InlineData(new object[] { Scenario.Memory, "root", "", false })]
        [InlineData(new object[] { Scenario.Memory, "", "root", false })]
        [InlineData(new object[] { Scenario.Memory, "wrong", "root", false })]
        [InlineData(new object[] { Scenario.Memory, "root", "wrong", false })]
        [InlineData(new object[] { Scenario.Memory, "wrong", "wrong", false })]
        [InlineData(new object[] { Scenario.Memory, "root", "root", true })]
        [InlineData(new object[] { Scenario.Database, null, null, false })]
        [InlineData(new object[] { Scenario.Database, "root", null, false })]
        [InlineData(new object[] { Scenario.Database, null, "root", false })]
        [InlineData(new object[] { Scenario.Database, "", "", false })]
        [InlineData(new object[] { Scenario.Database, "root", "", false })]
        [InlineData(new object[] { Scenario.Database, "", "root", false })]
        [InlineData(new object[] { Scenario.Database, "wrong", "root", false })]
        [InlineData(new object[] { Scenario.Database, "root", "wrong", false })]
        [InlineData(new object[] { Scenario.Database, "wrong", "wrong", false })]
        [InlineData(new object[] { Scenario.Database, "root", "root", true })]
        public void Login(string scenario, string username, string password, bool expected)
        {
            Scenario.Load(scenario);
            var im = Factory.Get<IdentityManager>();
            if (expected)
                Assert.True(im.Login(username, password), $"Login fail unexpected: username<{username}> password<{password}>");
            else
                Assert.False(im.Login(username, password), $"Login success unexpected: username<{username}> password<{password}>");
        }
        #endregion
        #region CheckFunctionAssignment
        [Theory]
        [InlineData(new object[] { new[] { Scenario.Memory, Scenario.Database },
            "root", "root",
            new[] { "CREATE", "DELETE" },
            new[] { typeof(Order) },
            "Verify that 'root' user can 'Create' and 'Delete' entities of type 'Order'",
            true })]
        [InlineData(new object[] { new[] { Scenario.Memory, Scenario.Database },
            "root", "root",
            new[] { "CREATE", "DELETE" },
            new[] { typeof(Order), typeof(Invoice) },
            "Verify that 'root' user can 'Create' and 'Delete' entities of type 'Order' and 'Invoice'",
            true })]
        [InlineData(new object[] { new[] { Scenario.Memory, Scenario.Database },
            "ca_sell", "ca_sell",
            new[] { "CREATE" },
            new[] { typeof(Order) },
            "Verify that 'California seller' user can NOT 'Create' entities of type 'Order'",
            false })]
        public void CheckFunctionAssignment(string[] scenarios, string username, string password, string[] functionsIds, Type[] types, string message, bool expected)
        {
            Printer.Print($"{message}");
            Printer.Print("");
            foreach (var scenario in scenarios)
            {
                Printer.Ident($"Scenario = { scenario}", () =>
                {
                    Scenario.Load(scenario);
                    var im = Factory.Get<IdentityManager>();
                    var functions = functionsIds.Select(id => GetById(id)).ToArray();
                    Assert.True(im.Login(username, password), $"Login fail unexpected: username<{username}> password<{password}>");
                    Printer.Ident("Parameters:", () =>
                    {
                        Printer.Print($"Username: {username}");
                        Printer.Print($"Functions: {functions.Aggregate("", (a, c) => a + c.Name + "·")}");
                        Printer.Print($"Types: {types.Aggregate("", (a, c) => a + c.Name + "·")}");
                    });
                    var strArgs = $"\r\nscenario<{scenario}>\r\nusername<{username}>\r\nfunctions<{functions.Aggregate("", (a, c) => a + c.Name + "·")}>\r\ntypes<{types.Aggregate("", (a, c) => a + c.Name + "·")}>";
                    if (expected)
                        Assert.True(
                            im.Current
                                .Can(functions)
                                .OfAllTypes(types)
                            , $"Function assignment failed unexpected: {strArgs}");
                    else
                        Assert.False(
                            im.Current
                                .Can(functions)
                                .OfAllTypes(types)
                            , $"Function assignment success unexpected: {strArgs}");
                });
            }
        }
        #endregion
        [Theory]
        [InlineData(new object[] { "Two discriminators of same type", new[] { Scenario.Memory, Scenario.Database }, "ca_sell", "ca_sell", typeof(Order), new[] { "READ" } })]
        public void Filter(string _, string[] scenarios, string username, string password, Type type, string[] functionsIds)
        {
            foreach (var scenario in scenarios)
            {
                Scenario.Load(scenario);
                var im = Factory.Get<IdentityManager>();
                var functions = functionsIds.Select(id => GetById(id)).ToArray();
                var rep = Factory.Get<IIdentityTestRepository>();
                Assert.True(im.Login(username, password), $"Login fail unexpected: username<{username}> password<{password}>");
                var strArgs = $"\r\nscenario<{scenario}>\r\nusername<{username}>";
                var dbSet = typeof(IIdentityTestRepository).GetMethod("GetByType").MakeGenericMethod(type).Invoke(rep, null);
                var res = (IEnumerable<object>)typeof(FuxionExtensions).GetMethod("AuthorizedTo").MakeGenericMethod(type).Invoke(null, new object[] { dbSet, functions });
                Assert.NotNull(res);
                Assert.True(res.Count() == 1, $"Must be '1' but have '{res.Count()}'");
            }
        }
        [Theory]
        [InlineData(new object[] { Scenario.Memory })]
        [InlineData(new object[] { Scenario.Database })]
        public void Filter_TwoDiscriminators(string scenario)
        {
            Scenario.Load(scenario);
            var im = Factory.Get<IdentityManager>();
            var rep = Factory.Get<IIdentityTestRepository>();

            Assert.True(im.Login("ca_sell", "ca_sell"), "Login unsuccessfull");

            var res = rep.Order.AuthorizedTo(Read);
            Assert.NotNull(res);
            Assert.True(res.Count() == 2);
        }
        [Theory]
        [InlineData(new object[] { Scenario.Memory })]
        [InlineData(new object[] { Scenario.Database })]
        public void Filter_OneDiscriminator(string scenario)
        {
            Scenario.Load(scenario);
            var im = Factory.Get<IdentityManager>();
            var rep = Factory.Get<IIdentityTestRepository>();

            Assert.True(im.Login("ca_sell", "ca_sell"), "Login unsuccessfull");

            var res = rep.Invoice.AuthorizedTo(Read);
            Assert.NotNull(res);
            //Assert.True(res.Count() == 2);
        }
    }
}
