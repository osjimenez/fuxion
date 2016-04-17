using Fuxion.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Licensing
{
    public class TimeLimitLicenseConstraint : LicenseConstraint
    {
        public TimeLimitLicenseConstraint(DateTime value) { Value = value; }
        public DateTime Value { get; private set; }
        public new bool Validate(out string validationMessage) {
            var res = base.Validate(out validationMessage);
            var tp = Factory.Get<ITimeProvider>();
            if (tp.GetUtcNow() > Value)
            {
                validationMessage = "Time limit expired";
                res = false;
            }
            return res;
        }
    }
}
