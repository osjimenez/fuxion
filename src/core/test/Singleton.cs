using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Test
{
    public class SingletonTest
    {
        [Fact]
        public void Singleton_AddAndGet()
        {
            var id = Guid.NewGuid();
            Singleton.Add(id);
            var res = Singleton.Get<Guid>();
            Assert.Equal(res, id);
        }
        [Fact]
        public void Singleton_AddAndGetWithKey()
        {
            var id = Guid.NewGuid();
            Singleton.Add("oka", id);
            var res = Singleton.Get<string>(id);
            Assert.Equal("oka", res);
        }
    }
}
