using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fuxion.Factories;
using System.Diagnostics;

namespace Fuxion.Identity
{
    public interface IPermission
    {
        IFunction Function { get; }
        IEnumerable<IScope> Scopes { get; }
        bool Value { get; }
        IRol Rol { get; }
    }
    public class PermissionEqualityComparer : IEqualityComparer<IPermission>
    {
        FunctionEqualityComparer funCom = new FunctionEqualityComparer();
        ScopeEqualityComparer scoCom = new ScopeEqualityComparer();
        public bool Equals(IPermission x, IPermission y)
        {
            return AreEquals(x, y);
        }

        public int GetHashCode(IPermission obj)
        {
            if (obj == null) return 0;
            return funCom.GetHashCode(obj.Function) ^ obj.Scopes.Select(s => scoCom.GetHashCode(s)).Aggregate(0, (a, c) => a ^ c) ^ obj.Value.GetHashCode();
        }
        bool AreEquals(object obj1, object obj2)
        {
            // If both are NULL, return TRUE
            if (Equals(obj1, null) && Equals(obj2, null)) return true;
            // If some of them is null, return FALSE
            if (Equals(obj1, null) || Equals(obj2, null)) return false;
            // If any of them are of other type, return FALSE
            if (!(obj1 is IPermission) || !(obj2 is IPermission)) return false;
            var per1 = (IPermission)obj1;
            var per2 = (IPermission)obj2;
            // Use 'Equals' to compare the ids
            return funCom.Equals(per1.Function, per2.Function) &&
                per1.Scopes.All(s => per2.Scopes.Any(s2 => scoCom.Equals(s, s2))) &&
                per2.Scopes.All(s => per1.Scopes.Any(s2 => scoCom.Equals(s, s2))) &&
                per1.Value == per2.Value;
        }
    }
}
