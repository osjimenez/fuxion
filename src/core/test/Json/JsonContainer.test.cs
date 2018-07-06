using Fuxion.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test.Json
{
    public class JsonContainerTest : BaseTest
    {
        public JsonContainerTest(ITestOutputHelper output) : base(output) { this.output = output; }
        ITestOutputHelper output;
        [Fact(DisplayName = "JsonContainer - Create")]
        public void Create()
        {
            var use = new UserMock
            {
                Name = "oka",
                Age = 23
            };
            var con = JsonContainer<string>.Create(use, "key");
			var con2 = JsonContainer<Type>.Create(use, use.GetType());
            var json = con.ToJson();
			var json2 = con2.ToJson();

            output.WriteLine(json);
			output.WriteLine(json2);

			con = json.FromJson<JsonContainer<string>>();
			con2 = json.FromJson<JsonContainer<Type>>();
			Assert.Equal("key", con.Key);
			Assert.Equal(typeof(UserMock), con2.Key);
			use = con.As<UserMock>();

			Assert.Equal("oka", use.Name);
            Assert.Equal(23, use.Age);
        }
    }
    public class UserMock
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
