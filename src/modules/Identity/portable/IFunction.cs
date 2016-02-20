using System;
using Fuxion.Identity.Helpers;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace Fuxion.Identity
{
    public interface IInclusive<T>
    {
        IEnumerable<T> Inclusions { get; }
    }
    //public interface IInclusive<T<TId>, TId> : IInclusive
    //{
    //    new IEnumerable<T<TId>> Inclusions { get; }
    //}
    public interface IExclusive<T>
    {
        IEnumerable<T> Exclusions { get; }
    }
    //public interface IExclusive<TId> : IExclusive
    //{
    //    new IEnumerable<IFunction<TId>> Exclusions { get; }
    //}
    public interface IFunction : IInclusive<IFunction>, IExclusive<IFunction>
    {
        object Id { get; }
        string Name { get; }
        //IEnumerable<IFunction> Inclusions { get; }
        //IEnumerable<IFunction> Exclusions { get; }
    }
    public interface IFunction<TId> : IFunction, IInclusive<IFunction<TId>>, IExclusive<IFunction<TId>>
    {
        new TId Id { get; }
        //new IEnumerable<IFunction<TId>> Inclusions { get; }
        //new IEnumerable<IFunction<TId>> Exclusions { get; }
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
        private static IEnumerable<T> GetAllInclusions<T>(this IInclusive<T> me, List<T> progress)
        {
            var res = progress;
            if (me.Inclusions == null) return res;
            var fs = me.Inclusions.Except(res).ToList();
            res.AddRange(fs);
            foreach (var f in fs) GetAllInclusions((IInclusive<T>)f, res);
            return res;
        }
        internal static IEnumerable<T> GetAllInclusions<T>(this IInclusive<T> me)
        {
            return GetAllInclusions(me, new List<T>(new[] { (T)me }));
        }
        private static IEnumerable<T> GetAllExclusions<T>(this IExclusive<T> me, List<T> progress)
        {
            var res = progress;
            if (me.Exclusions == null) return res;
            var fs = me.Exclusions.Except(res).ToList();
            res.AddRange(fs);
            foreach (var f in fs) GetAllExclusions((IExclusive<T>)f, res);
            return res;
        }
        internal static IEnumerable<T> GetAllExclusions<T>(this IExclusive<T> me)
        {
            return GetAllExclusions(me, new List<T>(new[] { (T)me }));
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
            // Compare the ids
            return Comparer.AreEquals(fun1.Id, fun2.Id);
        }
    }
    public static class Functions
    {
        static Functions()
        {
            Read = new Function("READ");
            Edit = new Function("EDIT");
            Create = new Function("CREATE");
            Delete = new Function("DELETE");
            Manage = new Function("MANAGE");
            Admin = new Function("ADMIN");
            ((Function)Read).Exclusions = new[] { Edit }.Cast<IFunction<string>>();

            ((Function)Edit).Inclusions = new[] { Read }.Cast<IFunction<string>>();
            ((Function)Edit).Exclusions = new[] { Create, Delete }.Cast<IFunction<string>>();

            ((Function)Create).Inclusions = new[] { Edit }.Cast<IFunction<string>>();
            ((Function)Create).Exclusions = new[] { Manage }.Cast<IFunction<string>>();

            ((Function)Delete).Inclusions = new[] { Edit }.Cast<IFunction<string>>();
            ((Function)Delete).Exclusions = new[] { Manage }.Cast<IFunction<string>>();

            ((Function)Manage).Inclusions = new[] { Create, Delete }.Cast<IFunction<string>>();
            ((Function)Manage).Exclusions = new[] { Admin }.Cast<IFunction<string>>();

            ((Function)Admin).Inclusions = new[] { Manage }.Cast<IFunction<string>>();
            dic = new Dictionary<string, IFunction>
            {
                [((Function)Read).Id] = Read,
                [((Function)Edit).Id] = Edit,
                [((Function)Create).Id] = Create,
                [((Function)Delete).Id] = Delete,
                [((Function)Manage).Id] = Manage,
                [((Function)Admin).Id] = Admin,
            };
        }
        static Dictionary<string, IFunction> dic;
        public static IFunction Read { get; private set; }
        public static IFunction Edit { get; private set; }
        public static IFunction Create { get; private set; }
        public static IFunction Delete { get; private set; }
        public static IFunction Manage { get; private set; }
        public static IFunction Admin { get; private set; }
        public static IFunction GetById(string id)
        {
            return dic[id];
        }
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

            public IEnumerable<IFunction<string>> Inclusions { get; internal set; }
            public IEnumerable<IFunction<string>> Exclusions { get; internal set; }
            //IEnumerable<IFunction> IFunction.Inclusions { get { return Inclusions; } }
            //IEnumerable<IFunction> IFunction.Exclusions { get { return Exclusions; } }

            IEnumerable<IFunction> IInclusive<IFunction>.Inclusions
            {
                get
                {
                    return Inclusions;
                }
            }

            IEnumerable<IFunction> IExclusive<IFunction>.Exclusions
            {
                get
                {
                    return Exclusions;
                }
            }
        }
        //static IFunction _Read = new Function("READ");
        //public static IFunction Read { get { return new Function("READ"    , null                       , new[] { (Function)Edit }); } }
        //public static IFunction Edit { get { return new Function("EDIT"    , new[] { (Function)Read }   , new[] { (Function)Create, (Function)Delete }); } }
        //public static IFunction Create { get { return new Function("CREATE", new[] { (Function)Edit }   , new[] { (Function)Manage }); } }
        //public static IFunction Delete { get { return new Function("DELETE", new[] { (Function)Edit }   , new[] { (Function)Manage }); } }
        //public static IFunction Manage{ get { return new Function("MANAGE" , new[] { (Function)Create   , (Function)Delete }, new[] { (Function)Admin }); } }
        //public static IFunction Admin { get { return new Function("DELETE" , new[] { (Function)Manage }); } }
    }
}
