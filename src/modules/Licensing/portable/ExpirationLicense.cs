using Fuxion.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Licensing
{
    public class ExpirationLicense : License
    {
        public ExpirationLicense()
        {
            ExpirationUtcTime = DateTime.UtcNow.AddYears(1);
        }
        public DateTime ExpirationUtcTime { get; private set; }
        protected internal override bool Validate(out string validationMessage)
        {
            var res = base.Validate(out validationMessage);
            var tp = Factory.Get<ITimeProvider>();
            if (tp.GetUtcNow() > ExpirationUtcTime)
            {
                validationMessage = "License expired";
                res = false;
            }
            return res;
        }
    }
    public class DeactivationLicense : License {
        public DeactivationLicense()
        {
            DeactivationUtcTime = new TimeLicenseConstraint();
        }
        public TimeLicenseConstraint DeactivationUtcTime { get; private set; }
        protected internal override bool Validate(out string validationMessage)
        {
            var res = base.Validate(out validationMessage);
            res = DeactivationUtcTime.Validate(out validationMessage);
            return res;
        }
    }
    public class LicenseConstraint {
        protected internal virtual bool Validate(out string validationMessage)
        {
            validationMessage = "Success";
            return true;
        }
    }
    public class TimeLicenseConstraint : LicenseConstraint {
        public TimeLicenseConstraint()
        {
            Value = DateTime.UtcNow.AddMonths(1);
        }
        public DateTime Value { get; private set; }
        protected internal override bool Validate(out string validationMessage)
        {
            var res = base.Validate(out validationMessage);
            var tp = Factory.Get<ITimeProvider>();
            if (tp.GetUtcNow() > Value)
            {
                validationMessage = "License deactivated";
                res = false;
            }
            return res;
        }
    }
}
