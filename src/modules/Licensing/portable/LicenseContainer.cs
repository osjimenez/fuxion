using Newtonsoft.Json.Linq;
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
        public LicenseData Data { get; set; }
        public string Signature { get; set; }

        //public JRaw Content { get; set; }
        //public T ContentAs<T>()
        //{
        //    return Content.Value.ToString().FromJson<T>();
        //}
        //public bool ContentIs<T>()
        //{
        //    try
        //    {
        //        Content.Value.ToString().FromJson<T>();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
    }
}
