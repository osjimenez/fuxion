using Fuxion.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Model
{
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    [TypeDiscriminated("Base")]
    public abstract class Base<TNotifier> : Notifier<TNotifier>, INotifier<TNotifier>
        where TNotifier : Base<TNotifier>
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
