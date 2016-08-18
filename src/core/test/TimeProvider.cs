using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fuxion.Test
{
    public class TimeProvider
    {
        [Fact]
        public void Init()
        {
            InternetTimeProvider itp = new InternetTimeProvider();
            //itp.ServerType = InternetTimeServerType.Web;
            //itp.ServerAddress = "http://www.microsoft.com";
            var res = itp.GetUtcNowWithOffset();

            Debug.WriteLine("");
        }
    }
}
