using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Licensing
{
    public class KeyModelLicenseConstraint : LicenseConstraint
    {
        public KeyModelLicenseConstraint(string? key) { Key = key; }
        public string? Key { get; private set; }
        public bool Validate(out string validationMessage, string model)
        {
            var res = base.Validate(out validationMessage);
            if (Key != model)
            {
                validationMessage = "License and model differ";
                res = false;
            }
            return res;
        }
    }
}
