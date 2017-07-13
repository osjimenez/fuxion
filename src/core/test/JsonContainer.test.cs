using Fuxion.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test
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
            var json = con.ToJson();

            output.WriteLine(json);

            con = json.FromJson<JsonContainer<string>>();
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
