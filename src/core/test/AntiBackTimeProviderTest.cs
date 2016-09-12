using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test
{
    public class AntiBackTimeProviderTest
    {
        public AntiBackTimeProviderTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        [Fact]
        public void AntiBackTimeProvider_CheckConsistency()
        {
            new AntiBackTimeProvider(new MockStorageTimeProvider().Transform(s =>
                {
                    s.SaveUtcTime(DateTime.UtcNow);
                    return s;
                }))
                .CheckConsistency(output);
        }
        [Fact]
        public void RegistryStorageTimeProvider_CheckConsistency()
        {
            new AntiBackTimeProvider(new RegistryStorageTimeProvider().Transform(s =>
                {
                    s.SaveUtcTime(DateTime.UtcNow);
                    return s;
                }))
                .CheckConsistency(output);
        }
        [Fact]
        public void AntiBackTimeProvider_BackTimeException()
        {
            var mock = new MockTimeProvider();
            var attp = new AntiBackTimeProvider(new RegistryStorageTimeProvider().Transform(s =>
                    {
                        s.SaveUtcTime(DateTime.UtcNow);
                        return s;
                    }));
            mock.SetOffset(TimeSpan.FromDays(-1));
            Assert.Throws<BackTimeException>(() => attp.UtcNow());
        }

    }
    public class MockStorageTimeProvider : StorageTimeProvider
    {
        DateTime value;
        public override DateTime GetUtcTime()
        {
            return value;
        }
        public override void SaveUtcTime(DateTime time)
        {
            value = time.ToUniversalTime();
        }
    }
}
