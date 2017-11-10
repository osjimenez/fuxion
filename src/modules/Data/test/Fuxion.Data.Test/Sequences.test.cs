using System;
using Fuxion.Test;
using Xunit;
using Xunit.Abstractions;
using System.Data.Entity;
using Fuxion.Data;
using Fuxion.Data.Test.Migrations;

namespace Fuxion.Data.Test
{
    [Collection("Sequences")]
    public class Sequences : BaseTest, IDisposable
    {
        public Sequences(ITestOutputHelper output) : base(output) {
            con = new TestContext();
        }
        public void Dispose()
        {
            con.Database.Delete();
            con.Dispose();
        }
        TestContext con;

        [Fact]
        public void CreateAndDeleteSequence()
        {
            con.CreateSequence("T");
            con.DeleteSequence("T");
        }
        [Fact]
        public void GetSequenceValue()
        {
            con.CreateSequence("T");
            var val = con.GetSequenceValue("T");
            Assert.Equal(1, val);
            val = con.GetSequenceValue("T");
            Assert.Equal(2, val);
            val = con.GetSequenceValue("T", false);
            Assert.Equal(2, val);
            con.DeleteSequence("T");
        }
        [Fact]
        public void SetSequenceValue()
        {
            con.CreateSequence("T");
            var val = con.GetSequenceValue("T");
            Assert.Equal(1, val);
            con.SetSequenceValue("T", 12);
            val = con.GetSequenceValue("T", false);
            Assert.Equal(12, val);
            con.DeleteSequence("T");
        }

    }
    public class TestContext : DbContext
    {
        public TestContext() : base("Data Source=(local);Initial Catalog=FuxionDataTest;Integrated Security=True") {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<TestContext, Configuration>());
            Database.SetInitializer(new DropCreateDatabaseAlways<TestContext>());
            
        }
    }
}
