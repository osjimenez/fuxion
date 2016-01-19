using System;
using Fuxion.Identity.Helpers;
using System.Reflection;
using System.Collections.Generic;

namespace Fuxion.Identity
{
    public interface IFunction
    {
        object Id { get; }
        string Name { get; }
    }
    public interface IFunction<TId> : IFunction
    {
        new TId Id { get; }
    }
    public static class FunctionExtensions
    {
        internal static object GetDefaultValue(this Type t)
        {
            if (t.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(t) == null)
                return Activator.CreateInstance(t);
            else
                return null;
        }
        public static bool IsValid(this IFunction me) { return !Comparer.AreEquals(me.Id,me.Id?.GetType().GetDefaultValue()) && !string.IsNullOrWhiteSpace(me.Name); }
    }
    class FunctionEqualityComparer : IEqualityComparer<IFunction>
    {
        public bool Equals(IFunction x, IFunction y)
        {
            return AreEquals(x, y);
        }

        public int GetHashCode(IFunction obj)
        {
            if (obj == null) return 0;
            return obj.Id.GetHashCode();
        }
        static bool AreEquals(object obj1, object obj2)
        {
            // If both are NULL, return TRUE
            if (Equals(obj1, null) && Equals(obj2, null)) return true;
            // If some of them is null, return FALSE
            if (Equals(obj1, null) || Equals(obj2, null)) return false;
            // If any of them are of other type, return FALSE
            if (!(obj1 is IFunction) || !(obj2 is IFunction)) return false;
            var fun1 = (IFunction)obj1;
            var fun2 = (IFunction)obj2;
            // Use 'Equals' to compare the ids
            return Comparer.AreEquals(fun1.Id, fun2.Id);
        }
    }
}
