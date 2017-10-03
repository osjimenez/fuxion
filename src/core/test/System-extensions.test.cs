using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test
{
    public class SystemExtensionsTest
    {
        public SystemExtensionsTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;

        [Fact(DisplayName = "System - IsNullOrDefault")]
        public void IsNullOrDefaultTest()
        {
            string s = null;
            Assert.True(s.IsNullOrDefault());
            s = "";
            Assert.False(s.IsNullOrDefault());
            int i = 0;
            Assert.True(i.IsNullOrDefault());
            i = 1;
            Assert.False(i.IsNullOrDefault());
            Guid g = Guid.Empty;
            Assert.True(g.IsNullOrDefault());
            g = Guid.NewGuid();
            Assert.False(g.IsNullOrDefault());
            int? i2 = null;
            Assert.True(i2.IsNullOrDefault());
            i2 = null;
            Assert.True(i2.IsNullOrDefault());
            i2 = 1;
            Assert.False(i2.IsNullOrDefault());
        }
        #region CloneWithJson
        [Fact(DisplayName = "System - CloneWithJson")]
        public void CloneWithJsonTest()
        {
            Base b = new Derived();
            var res = b.CloneWithJson();
            output.WriteLine("res.GetType() = " + res.GetType().Name);
            Assert.Equal(nameof(Derived), res.GetType().Name);
        }
        class Base { }
        class Derived : Base { }
        #endregion
        #region Transform
        [Fact]
        public void TransfromTest()
        {
            var res = new TransformationSource
            {
                Integer = 123,
                String = "test"
            }.Transform(source => source.Integer);

            Assert.Equal(123, res);
        }
        class TransformationSource
        {
            public int Integer { get; set; }
            public string String { get; set; }
        }
        #endregion
    }
}
