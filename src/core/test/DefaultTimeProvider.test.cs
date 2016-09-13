using Fuxion.Threading.Tasks;
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
    public class DefaultTimeProviderTest
    {
        public DefaultTimeProviderTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        [Fact]
        public void DefaultTimeProvider_CheckConsistency()
        {
            new DefaultTimeProvider().CheckConsistency(output);
        }
    }
}
