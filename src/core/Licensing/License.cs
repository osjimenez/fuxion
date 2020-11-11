using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Licensing
{
    public abstract class License
    {
        protected internal virtual bool Validate([NotNull] out string validationMessage) {
            validationMessage = "Success";
            return true;
        }
        public DateTime SignatureUtcTime { get; set; }
    }
}
