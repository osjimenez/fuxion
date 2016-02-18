using System;
using Fuxion.Identity.Helpers;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace Fuxion.Identity
{
    public interface IFunction
    {
        object Id { get; }
        string Name { get; }
        IEnumerable<IFunction> Inclusions { get; }
        IEnumerable<IFunction> Exclusions { get; }
    }
    public interface IFunction<TId> : IFunction
    {
        new TId Id { get; }
        new IEnumerable<IFunction<TId>> Inclusions { get; }
        new IEnumerable<IFunction<TId>> Exclusions { get; }
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
        internal static IEnumerable<IFunction> GetAllInclusions(this IFunction function)
        {
            var res = new List<IFunction>();
            if (function.Inclusions != null)
            {
                res.AddRange(function.Inclusions.SelectMany(i => GetAllInclusions(i)));
            }
            return res;
        }
        internal static IEnumerable<IFunction> GetAllExclusions(this IFunction function)
        {
            var res = new List<IFunction>();
            if (function.Inclusions != null)
            {
                res.AddRange(function.Inclusions.SelectMany(i => GetAllExclusions(i)));
            }
            return res;
        }
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
    public static class Functions
    {
        [DebuggerDisplay("{" + nameof(Name) + "}")]
        class Function : IFunction<string>
        {
            public Function(string id, IEnumerable<Function> inclusions = null, IEnumerable<Function> exclusions = null)
            {
                Id = id;
                Name = id;
                Inclusions = inclusions;
                Exclusions = exclusions;
            }
            public string Id { get; private set; }
            object IFunction.Id { get { return Id; } }
            public string Name { get; private set; }

            public IEnumerable<IFunction<string>> Inclusions { get; private set; }
            public IEnumerable<IFunction<string>> Exclusions { get; private set; }
            IEnumerable<IFunction> IFunction.Inclusions { get { return Inclusions; } }
            IEnumerable<IFunction> IFunction.Exclusions { get { return Exclusions; } }
        }
        public static IFunction Read { get { return new Function("READ", null, new[] { (Function)Edit }); } }
        public static IFunction Edit { get { return new Function("EDIT", new[] { (Function)Read }, new[] { (Function)Create, (Function)Delete }); } }
        public static IFunction Create { get { return new Function("CREATE", new[] { (Function)Edit }, new[] { (Function)Manage }); } }
        public static IFunction Delete { get { return new Function("DELETE", new[] { (Function)Edit }, new[] { (Function)Manage }); } }
        public static IFunction Manage{ get { return new Function("MANAGE", new[] { (Function)Create, (Function)Delete }, new[] { (Function)Admin }); } }
        public static IFunction Admin { get { return new Function("DELETE", new[] { (Function)Manage }); } }
    }
}
