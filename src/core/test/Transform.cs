using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Test
{
    public class Transform
    {
        [Fact]
        public void Transfrom_First()
        {
            var res = new TransformationSource
            {
                Integer = 123,
                String = "test"
            }.Transform(source => source.Integer);

            Assert.Equal(res, 123);
        }
    }
    public class TransformationSource
    {
        public int Integer { get; set; }
        public string String { get; set; }
    }
}
