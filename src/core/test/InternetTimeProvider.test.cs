using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test
{
    public class InternetTimeProviderTest
    {
        public InternetTimeProviderTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        [Fact]
        public void InternetTimeProvider_CheckConsistency()
        {
            new InternetTimeProvider().CheckConsistency(output);
        }
    }
}
