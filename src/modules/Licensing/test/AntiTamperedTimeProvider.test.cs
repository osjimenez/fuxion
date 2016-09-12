using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Licensing.Test
{
    public class AntiTamperedTimeProviderTest
    {
        public AntiTamperedTimeProviderTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        string[] WebServersAddresses { get; } = new[]
{
            "http://www.google.com",
            "http://www.google.es",
            //"http://www.ooooooo-youtube.com",
            "http://www.microsoft.com",
            "http://www.yahoo.com",
            "http://www.amazon.com",
            "http://www.facebook.com",
            "http://www.twitter.com",
        };
        [Fact]
        public void AntiTamperedTimeProvider_CheckConsistency()
        {
            var atp = new AverageTimeProvider
            {
                MaxFailsPerTry = 1,
                RandomizedProvidersPerTry = WebServersAddresses.Length
            };
            foreach (var pro in WebServersAddresses.Select(address => new InternetTimeProvider
            {
                ServerAddress = address,
                ServerType = InternetTimeServerType.Web,
                Timeout = TimeSpan.FromSeconds(5)
            })) atp.AddProvider(pro, true, true);
            new AntiTamperedTimeProvider(atp, new AntiBackTimeProvider())
                .CheckConsistency(output);
        }
    }
}
