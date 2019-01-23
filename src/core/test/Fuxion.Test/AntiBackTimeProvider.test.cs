using Fuxion.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test
{
    public class AntiBackTimeProviderTest : BaseTest
    {
        public AntiBackTimeProviderTest(ITestOutputHelper output) : base(output)
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
            new AntiBackTimeProvider(new RegistryStoredTimeProvider().Transform(s =>
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
            var abtp = new AntiBackTimeProvider(new RegistryStoredTimeProvider().Transform(s =>
                    {
                        s.SaveUtcTime(DateTime.UtcNow);
                        return s;
                    }));
            abtp.TimeProvider = mock;
            abtp.Log = new XunitLog(output);
            mock.SetOffset(TimeSpan.FromDays(-1));
            Assert.Throws<BackTimeException>(() => abtp.UtcNow());
        }
    }
    public class MockStorageTimeProvider : StoredTimeProvider
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
