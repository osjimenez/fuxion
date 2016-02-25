using Fuxion.Identity.Test.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Fuxion.Identity.DatabaseEFTest.Scenario;
using static Fuxion.Identity.Functions;
using Xunit.Abstractions;
using Fuxion.Factories;
using Fuxion.Identity.Test;
using System.Data.Entity;
using System.Diagnostics;

namespace Fuxion.Identity.DatabaseEFTest
{
    public class IdentityManagerTest
    {
        public IdentityManagerTest(ITestOutputHelper output)
        {
            Printer.PrintAction = m =>
            {
                Debug.WriteLine(m);
                output.WriteLine(m);
            };
        }
#if DEBUG
        public const string scenarios = MEMORY+"·"+DATABASE;
#else
        public const string scenarios = MEMORY+"·"+DATABSE;
#endif
        #region Login
        [Theory]
        [InlineData(new object[] { "Username and password null, must fail", scenarios, null, null, false })]
        [InlineData(new object[] { "Password null, must fail", scenarios, "root", null, false })]
        [InlineData(new object[] { "Username null, must fail", scenarios, null, "root", false })]
        [InlineData(new object[] { "Username and password empty, must fail", scenarios, "", "", false })]
        [InlineData(new object[] { "Password empty, must fail", scenarios, "root", "", false })]
        [InlineData(new object[] { "Username empty, must fail", scenarios, "", "root", false })]
        [InlineData(new object[] { "Username and password wrong, must fail", scenarios, "wrong", "wrong", false })]
        [InlineData(new object[] { "Password wrong, must fail", scenarios, "root", "wrong", false })]
        [InlineData(new object[] { "Username wrong, must fail", scenarios, "wrong", "root", false })]
        [InlineData(new object[] { "Must be success", scenarios, "root", "root", true })]
        public void Login(string _, string scenarios, string username, string password, bool expected)
        {
            foreach (var scenario in scenarios.Split('·'))
            {
                Printer.Ident($"Scenario = { scenario}", () =>
                {
                    Load(scenario);
                    var im = Factory.Get<IdentityManager>();
                    if (expected)
                        Assert.True(im.Login(username, password), $"Login fail unexpected: username<{username}> password<{password}>");
                    else
                        Assert.False(im.Login(username, password), $"Login success unexpected: username<{username}> password<{password}>");
                });
            }
        }
        #endregion
        #region Check
        [Theory]
        [InlineData(new object[] {
            "Root can create and delete orders", scenarios,
            "root", "root",
            new[] { "CREATE", "DELETE" },
            new[] { typeof(Order) },
            "Verify that 'root' user can 'Create' and 'Delete' entities of type 'Order'",
            true })]
        [InlineData(new object[] {
            "Root can create and delete orders and invoices", scenarios,
            "root", "root",
            new[] { "CREATE", "DELETE" },
            new[] { typeof(Order), typeof(Invoice) },
            "Verify that 'root' user can 'Create' and 'Delete' entities of type 'Order' and 'Invoice'",
            true })]
        [InlineData(new object[] {
            "California seller can NOT create orders", scenarios,
            "ca_sell", "ca_sell",
            new[] { "CREATE" },
            new[] { typeof(Order) },
            "Verify that 'California seller' user can NOT 'Create' entities of type 'Order'",
            false })]
        public void Check(string _, string scenarios, string username, string password, string[] functionsIds, Type[] types, string message, bool expected)
        {
            Printer.Print($"{message}");
            Printer.Print("");
            foreach (var scenario in scenarios.Split('·'))
            {
                Printer.Ident($"Scenario = { scenario}", () =>
                {
                    Load(scenario);
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
        #region Filter
        [Theory]
        [InlineData(new object[] {
            "Two discriminators of same type", scenarios,
            "ca_sell", "ca_sell",
            new[] { READ },
            typeof(Demo),
            new[] { DemoList.DEMO1 },
            false,
            new string[] { } })]
        [InlineData(new object[] {
            "Two discriminators of distinct type", scenarios,
            "ca_sell", "ca_sell",
            new[] { READ },
            typeof(Order),
            new[] { SellOrderList.CA_SELL_ORDER, SellOrderList.NY_SELL_ORDER },
            false,
            new string[] { } })]
        [InlineData(new object[] {
            "One discriminator", scenarios,
            "ca_sell", "ca_sell",
            new[] { READ },
            typeof(Invoice),
            new[] { InvoiceList.SAL_INVOICE },
            false,
            new string[] { } })]
        public void Filter(string _, string scenarios, string username, string password, string[] functionsIds, Type type, string[] expectedIds, bool allowOtherResults, string[] unexpectedIds)
        {
            foreach (var scenario in scenarios.Split('·'))
            {
                Printer.Ident($"Scenario = { scenario}", () =>
                {
                    Load(scenario);
                    var im = Factory.Get<IdentityManager>();
                    var functions = functionsIds.Select(id => GetById(id)).ToArray();
                    var rep = Factory.Get<IIdentityTestRepository>();
                    Assert.True(im.Login(username, password), $"Login fail unexpected: username<{username}> password<{password}>");
                    var strArgs = $"\r\nscenario<{scenario}>\r\nusername<{username}>";
                    var dbSet = typeof(IIdentityTestRepository).GetMethod("GetByType").MakeGenericMethod(type).Invoke(rep, null);
                    var res = (IEnumerable<object>)typeof(FuxionExtensions).GetMethod("AuthorizedTo").MakeGenericMethod(type).Invoke(null, new object[] { dbSet, functions });
                    var list = res.ToList().Cast<Base>();
                    if (allowOtherResults)
                        Assert.True(list.Any(e => expectedIds.Contains(e.Id)), $"Some expected ids '{expectedIds.Aggregate("", (a, c) => a + c + "·")}' not found");
                    else
                        Assert.True(list.All(e => expectedIds.Contains(e.Id)), $"Strict expected ids '{expectedIds.Aggregate("", (a, c) => a + c + "·")}' not found");
                });
            }
        }
        #endregion
        [Fact]
        public void DemoTest()
        {
            Load(MEMORY);
            var im = Factory.Get<IdentityManager>();
            var rep = Factory.Get<IIdentityTestRepository>();
            im.Login("ca_sell", "ca_sell");
            //Assert.True(im.Current.Can(Read).OfType<Order>());
            var res = rep.Album.Where(o => o.Songs.AuthorizedTo(Edit).Any());
            Printer.Print("res.Count(): " + res.Count());
        }
        [Fact]
        public void DemoTest2()
        {
            Load(DATABASE);
            var im = Factory.Get<IdentityManager>();
            var rep = Factory.Get<IIdentityTestRepository>();
            im.Login("root", "root");
            Assert.True(im.Current.Can(Read).OfType<Order>());
            var res = rep.Album.Where(o => o.Songs.AuthorizedTo(Read).Any());
            Printer.Print("res.Count(): " + res.Count());
        }
    }
}
