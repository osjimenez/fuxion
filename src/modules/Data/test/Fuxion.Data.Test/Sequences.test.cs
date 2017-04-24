using System;
using Fuxion.Test;
using Xunit;
using Xunit.Abstractions;
using System.Data.Entity;
using Fuxion.Data;
namespace Fuxion.Data.Test
{
    [Collection("Sequences")]
    public class Sequences : BaseTest
    {
        public Sequences(ITestOutputHelper output) : base(output) { this.output = output; }
        ITestOutputHelper output;
        [Fact]
        public void CreateAndDeleteSequence()
        {
            var con = new TestContext();
            con.CreateSequence("T");
            con.DeleteSequence("T");
        }
        [Fact]
        public void GetSequenceValue()
        {
            var con = new TestContext();
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
            var con = new TestContext();
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
        public TestContext() : base("DefaultConnection") { }
    }
}
