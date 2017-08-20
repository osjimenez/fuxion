using System.Collections.Generic;
using Fuxion.Identity.Helpers;
using System.Reflection;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Collections;
using System.Runtime.CompilerServices;
using static Fuxion.Identity.IdentityExtensions;
namespace Fuxion.Identity
{
    public interface IRol
    {
        string Name { get; }
        IEnumerable<IGroup> Groups { get; }
        IEnumerable<IPermission> Permissions { get; }
    }
}
