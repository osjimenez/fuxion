using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Fuxion.Test
{
    public class AverageTimeProviderTest
    {
        public AverageTimeProviderTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        ITestOutputHelper output;
        string[] NtpServersAddresses { get; } = new[]
{
            // From NIST - http://tf.nist.gov/tf-cgi/servers.cgi
            "time-a.nist.gov",
            "time-b.nist.gov",
            "time-c.nist.gov",
            "time-d.nist.gov",
            "nist1-macon.macon.ga.us",
            "wolfnisttime.com",
            "nist.netservicesgroup.com",
            "nisttime.carsoncity.k12.mi.us",
            "nist1-lnk.binary.net",
            "wwv.nist.gov",
            "time-a.timefreq.bldrdoc.gov",
            "time-b.timefreq.bldrdoc.gov",
            "time-c.timefreq.bldrdoc.gov",
            "time.nist.gov",
            "utcnist.colorado.edu",
            "utcnist2.colorado.edu",
            "ntp-nist.ldsbc.net",
            "time-nw.nist.gov",
            "nist-time-server.eoni.com",
            "nist-time-server.eoni.com",

            // From GOOGLE
            "time1.google.com",
            "time2.google.com",
            "time3.google.com",
            "time4.google.com",

            // From MICROSOFT - https://www.google.es/search?q=microsoft+time+servers&rlz=1C1ASUM_enES701ES701&oq=microsoft+time+servers&aqs=chrome..69i57j0l5.3247j0j4&sourceid=chrome&ie=UTF-8
            "time.windows.com",
            "time-nw.nist.gov",

            // AMAZON - http://stackoverflow.com/questions/29418250/is-there-an-ntp-server-i-should-be-using-when-using-amazons-ec2-service-to-comb
            "server 0.amazon.pool.ntp.org iburst",
            "server 1.amazon.pool.ntp.org iburst",
            "server 2.amazon.pool.ntp.org iburst",
            "server 3.amazon.pool.ntp.org iburst",

            // University of Colorado
            "utcnist2.colorado.edu",

            // From POOL.ORG - http://www.pool.ntp.org/zone/europe
            "0.europe.pool.ntp.org",
            "1.europe.pool.ntp.org",
            "2.europe.pool.ntp.org",
            "3.europe.pool.ntp.org",
            "es.pool.ntp.org",
        };
        string[] WebServersAddresses { get; } = new[]
        {
            "http://www.google.com",
            "http://www.google.es",
            "http://www.youtube.com",
            "http://www.microsoft.com",
            "http://www.yahoo.com",
            "http://www.amazon.com",
            "http://www.facebook.com",
            "http://www.twitter.com",
        };
        [Fact]
        public async Task CheckConsistency()
        {
            new AverageTimeProvider()
                .Transform(p => {
                    foreach (var add in WebServersAddresses)
                        p.AddProvider(new InternetTimeProvider
                        {
                            ServerAddress = add,
                            ServerType = InternetTimeServerType.Web,
                            Timeout = TimeSpan.FromSeconds(15)
                        });
                    p.RandomizedProvidersPerTry = 5;
                    p.MaxFailsPerTry = 0;
                    return p;
                })
                .CheckConsistency(output);
        }
    }
}
