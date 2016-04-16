using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PCLCrypto.WinRTCrypto;
namespace Fuxion.Licensing
{
    public class LicenseContainer
    {
        internal LicenseData Data { get; set; }
        public string Signature { get; set; }
    }
}
