using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PCLCrypto.WinRTCrypto;
namespace Fuxion.Srm
{
    public class License
    {
        public LicenseData Data { get; set; }
        public string Sign { get; set; }
    }
}
