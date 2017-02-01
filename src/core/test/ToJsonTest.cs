using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Test
{
    public class SerializableTest
    {
        public int Entero { get; set; }
        public string Cadena { get; set; }
    }
    public class ToJsonTest
    {
        [Fact]
        public void FormatTest()
        {
            Guid id = Guid.NewGuid();
            var test = new SerializableTest
            {
                Entero = 123,
                Cadena = "oka"
            };

            var i = id.ToJson();
            var ii = JsonConvert.SerializeObject(id);
            var itok = JToken.Parse(ii);

            var t = test.ToJson();
            var tt = JsonConvert.SerializeObject(test);
            var ttok = JToken.Parse(tt);



            Debug.WriteLine("");

        }
    }
}
