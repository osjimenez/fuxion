using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test
{
    public class CustomizableTimeProviderTest
    {
        public CustomizableTimeProviderTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        [Fact]
        public void CheckConsistency()
        {
            new CustomizableTimeProvider().CheckConsistency(output);
        }
    }
}
namespace Fuxion { 
    /// <summary>
    /// 
    //   TODO CustomizableTimeProvider
    ///
    /// Allow to provide base time from other ITimeProvider
    /// Allow freeze time
    /// Allow set base time explicitly or use an offset from base time
    /// </summary>
    public class CustomizableTimeProvider : ITimeProvider
    {
        public bool IsFreezed { get; private set; }
        public void Freeze() { IsFreezed = true; }
        public DateTime Now()
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset NowOffsetted()
        {
            throw new NotImplementedException();
        }

        public DateTime UtcNow()
        {
            throw new NotImplementedException();
        }
    }
}
