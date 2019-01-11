using Fuxion.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test
{
    public class CachedTimeProviderTest
    {
        public CachedTimeProviderTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        [Fact]
        public void CachedTimeProvider_CheckConsistency()
        {
            new CachedTimeProvider(new LocalMachinneTimeProvider())
                .CheckConsistency(output);
        }
        [Fact]
        public void CachedTimeProvider_CacheTest()
        {
            var ctp = new CachedTimeProvider(new LocalMachinneTimeProvider())
            {
                Log = new XunitLog(output),
                ExpirationInterval = TimeSpan.FromSeconds(1)
            };

            bool fromCache;

            ctp.UtcNow(out fromCache);
            Assert.False(fromCache);
            ctp.UtcNow(out fromCache);
            Assert.True(fromCache);

            Thread.Sleep(1000);

            ctp.UtcNow(out fromCache);
            Assert.False(fromCache);
            ctp.UtcNow(out fromCache);
            Assert.True(fromCache);
        }
    }
}
