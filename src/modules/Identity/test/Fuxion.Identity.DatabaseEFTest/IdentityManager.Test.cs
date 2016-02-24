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
using System.Linq.Expressions;
using Xunit.Extensions;
using System.Security.Permissions;
using Fuxion.Identity.Test;
using System.Collections;
using SimpleInjector;
using Fuxion.Repositories;

namespace Fuxion.Identity.DatabaseEFTest
{
    public class Scenario
    {
        public const string Database = nameof(Database);
        public const string Memory = nameof(Memory);
        public static void Load(string key)
        {
            if(key == Memory)
            {
                Factory.ClearPipe();
                var con = new Container();
                con.RegisterSingleton<IPasswordProvider>(new PasswordProvider());
                var rep = new IdentityMemoryTestRepository();
                con.RegisterSingleton<IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>>(rep);
                con.RegisterSingleton<IIdentityTestRepository>(rep);
                con.RegisterSingleton<IdentityManager>();
                var fac = new SimpleInjectorFactory(con);
                Factory.AddToPipe(fac);
            }else if (key == Database)
            {
                Factory.ClearPipe();
                var con = new Container();
                con.RegisterSingleton<IPasswordProvider>(new PasswordProvider());
                var rep = new IdentityDatabaseEFTestRepository();
                rep.InitializeData();
                con.RegisterSingleton<IKeyValueRepository<IdentityKeyValueRepositoryValue, string, IIdentity>>(rep);
                con.RegisterSingleton<IIdentityTestRepository>(rep);
                con.RegisterSingleton<IdentityManager>();
                var fac = new SimpleInjectorFactory(con);
                Factory.AddToPipe(fac);
            }
            else throw new NotImplementedException($"El escenario '{key}' no esta soportado");
        }
    }
    public class IdentityManagerTest
    {
        //private readonly ITestOutputHelper output;
        public IdentityManagerTest(ITestOutputHelper output)
        {
            Printer.PrintAction = m => output.WriteLine(m);
            TypeDiscriminator.KnownTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith("Fuxion"))
                .SelectMany(a => a.DefinedTypes).ToArray();
        }
        [Theory]
        [InlineData(new object[] { Scenario.Memory })]
        [InlineData(new object[] { Scenario.Database })]
        public void Login(string scenario)
        {
            Scenario.Load(scenario);
            var im = Factory.Get<IdentityManager>();
            // Check when null values
            Assert.False(im.Login(null, null));
            Assert.False(im.Login("root", null));
            Assert.False(im.Login(null, "root"));
            // Check when empty values
            Assert.False(im.Login("", ""));
            Assert.False(im.Login("root", ""));
            Assert.False(im.Login("", "root"));
            // Check when wrong values
            Assert.False(im.Login("wrong", "root"));
            Assert.False(im.Login("root", "wrong"));
            // Check when success
            Assert.True(im.Login("root", "root"));
        }
        [Theory]
        [InlineData(new object[] { Scenario.Memory })]
        [InlineData(new object[] { Scenario.Database })]
        public void CheckFunctionAssigned(string scenario)
        {
            Scenario.Load(scenario);
            var im = Factory.Get<IdentityManager>();

            // Login
            if (!im.IsAuthenticated)
                Assert.True(im.Login("root", "root"));
            // Check if can create & delete objects of type Order
            Assert.True(
                im.Current
                    .Can(Create, Delete)
                    .OfType<Order>());
            // Check if can create & delete objects of types Order AND Invoice
            Assert.True(
                im.Current
                    .Can(Create, Delete)
                    .OfAllTypes<Order, Invoice>());
            return;
        }
        [Theory]
        [InlineData(new object[] { Scenario.Memory })]
        [InlineData(new object[] { Scenario.Database })]
        public void Filter_TwoDiscriminatorsOfSameType(string scenario)
        {
            Scenario.Load(scenario);
            var im = Factory.Get<IdentityManager>();
            var rep = Factory.Get<IIdentityTestRepository>();

            Assert.True(im.Login("ca_sell", "ca_sell"), "Login unsuccessfull");

            var res = rep.Demo.AuthorizedTo(Read);
            Assert.NotNull(res);
            Assert.True(res.Count() == 1);
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
