using Fuxion.ComponentModel;
using Fuxion.Identity.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Identity.Test.Dvo
{
    public interface IBaseDvo<TNotifier> : INotifier<TNotifier>
        where TNotifier : IBaseDvo<TNotifier>
    { }
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    [TypeDiscriminated(TypeDiscriminatorIds.Base)]
    public abstract class BaseDvo<TNotifier> : Notifier<TNotifier>, IBaseDvo<TNotifier>
        where TNotifier : BaseDvo<TNotifier>
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}