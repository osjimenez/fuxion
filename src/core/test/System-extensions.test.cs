using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Fuxion.Resources;
namespace Fuxion.Test
{
    public class SystemExtensionsTest
    {
        public SystemExtensionsTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;

        [Fact(DisplayName = "Object - IsNullOrDefault")]
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
        [Fact(DisplayName = "Object - Transform")]
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
        #region TimeSpan
        [Fact(DisplayName = "TimeSpan - ToTimeString")]
        public void TimeSpan_ToTimeString()
        {
            var res = TimeSpan.Parse("1.18:53:58.1234567").ToTimeString();
            Assert.Contains($"1 {Strings.day}", res);
            Assert.Contains($"18 {Strings.hours}", res);
            Assert.Contains($"53 {Strings.minutes}", res);
            Assert.Contains($"58 {Strings.seconds}", res);
            Assert.Contains($"123 {Strings.milliseconds}", res);

            res = TimeSpan.Parse("1.18:53:58.1234567").ToTimeString(3);
            Assert.Contains($"1 {Strings.day}", res);
            Assert.Contains($"18 {Strings.hours}", res);
            Assert.Contains($"53 {Strings.minutes}", res);
            Assert.DoesNotContain($"58 {Strings.seconds}", res);
            Assert.DoesNotContain($"123 {Strings.milliseconds}", res);

            res = TimeSpan.Parse("0.18:53:58.1234567").ToTimeString(3);
            Assert.DoesNotContain($"0 {Strings.day}", res);
            Assert.Contains($"18 {Strings.hours}", res);
            Assert.Contains($"53 {Strings.minutes}", res);
            Assert.Contains($"58 {Strings.seconds}", res);
            Assert.DoesNotContain($"123 {Strings.milliseconds}", res);

            res = TimeSpan.Parse("1.18:53:58.1234567").ToTimeString(6);
            output.WriteLine("ToTimeString: "+res);

            // Only letters

            res = TimeSpan.Parse("1.18:53:58.1234567").ToTimeString(onlyLetters: true);
            Assert.Contains($"1 d", res);
            Assert.Contains($"18 h", res);
            Assert.Contains($"53 m", res);
            Assert.Contains($"58 s", res);
            Assert.Contains($"123 ms", res);

            res = TimeSpan.Parse("1.18:53:58.1234567").ToTimeString(3, true);
            Assert.Contains($"1 d", res);
            Assert.Contains($"18 h", res);
            Assert.Contains($"53 m", res);
            Assert.DoesNotContain($"58 s", res);
            Assert.DoesNotContain($"123 ms", res);

            res = TimeSpan.Parse("0.18:53:58.1234567").ToTimeString(3, true);
            Assert.DoesNotContain($"0 d", res);
            Assert.Contains($"18 h", res);
            Assert.Contains($"53 m", res);
            Assert.Contains($"58 s", res);
            Assert.DoesNotContain($"123 ms", res);

            res = TimeSpan.Parse("1.18:53:58.1234567").ToTimeString(6, true);
            output.WriteLine("ToTimeString (onlyLetters): " + res);
        }
        #endregion
    }
}
