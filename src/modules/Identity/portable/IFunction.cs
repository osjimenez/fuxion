using System;
using Fuxion.Identity.Helpers;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Fuxion.Math.Graph;

namespace Fuxion.Identity
{
    public interface IInclusive<T>
    {
        IEnumerable<T> Inclusions { get; }
    }
    public interface IExclusive<T>
    {
        IEnumerable<T> Exclusions { get; }
    }
    public interface IFunction : IInclusive<IFunction>, IExclusive<IFunction>
    {
        object Id { get; }
        string Name { get; }
    }
    public interface IFunction<TId> : IFunction, IInclusive<IFunction>, IExclusive<IFunction>
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
        private static IEnumerable<T> GetAllInclusions<T>(this IInclusive<T> me, List<T> progress)
        {
            var res = progress;
            if (me.Inclusions == null) return res;
            var fs = me.Inclusions.Except(res).ToList();
            res.AddRange(fs);
            foreach (var f in fs) GetAllInclusions((IInclusive<T>)f, res);
            return res;
        }
        public static IEnumerable<T> GetAllInclusions<T>(this IInclusive<T> me)
        {
            //return GetAllInclusions(me, new List<T>(new[] { (T)me }));
            return GetAllInclusions(me, new List<T>(new T[] { }));
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
        public static IEnumerable<T> GetAllExclusions<T>(this IExclusive<T> me)
        {
            //return GetAllExclusions(me, new List<T>(new[] { (T)me }));
            return GetAllExclusions(me, new List<T>(new T[] { }));
        }
        public static void Print(this IEnumerable<IFunction> me, PrintMode mode)
        {
            switch (mode)
            {
                case PrintMode.OneLine:
                    foreach (var fun in me)
                    {
                        Printer.Print(fun.Id.ToString() + "-" + fun.Name);
                    }
                    break;
                case PrintMode.PropertyList:
                    break;
                case PrintMode.Table:
                    //var valueLength = me.Select(p => p.Value.ToString().Length).Union(new[] { "VALUE".Length }).Max();
                    //var functionLength = me.Select(p => p.Function.Name.ToString().Length).Union(new[] { "FUNCTION".Length }).Max();
                    //var typeLength = new[] { "TYPE".Length }.Concat(me.SelectMany(p => p.Scopes.Select(s => s.Discriminator.TypeName.Length))).Max();
                    //var nameLength = new[] { "NAME".Length }.Concat(me.SelectMany(p => p.Scopes.Select(s => s.Discriminator.Name.Length))).Max();
                    //var propagationLength = new[] { "PROPAGATION".Length }.Concat(me.SelectMany(p => p.Scopes.Select(s => s.Propagation.ToString().Length))).Max();

                    //Printer.Print("┌" + ("".PadRight(valueLength, '─')) + "┬" + ("".PadRight(functionLength, '─')) + "┬" + ("".PadRight(typeLength, '─')) + "┬" + ("".PadRight(nameLength, '─')) + "┬" + ("".PadRight(propagationLength, '─')) + "┐");
                    //if (me.Any())
                    //{
                    //    Printer.Print("│" + ("VALUE".PadRight(valueLength, ' ')) + "│" + ("FUNCTION".PadRight(functionLength, ' ')) + "│" + ("TYPE".PadRight(typeLength, ' ')) + "│" + ("NAME".PadRight(nameLength, ' ')) + "│" + ("PROPAGATION".PadRight(propagationLength, ' ')) + "│");
                    //    Printer.Print("├" + ("".PadRight(valueLength, '─')) + "┼" + ("".PadRight(functionLength, '─')) + "┼" + ("".PadRight(typeLength, '─')) + "┼" + ("".PadRight(nameLength, '─')) + "┼" + ("".PadRight(propagationLength, '─')) + "┤");
                    //}

                    //foreach (var per in me)
                    //{
                    //    var list = per.Scopes.ToList();
                    //    if (list.Count == 0)
                    //    {
                    //        Printer.Print("│" +
                    //                per.Value.ToString().PadRight(valueLength, ' ') + "│" +
                    //                per.Function.Name.PadRight(functionLength, ' ') + "│" +
                    //                ("".PadRight(typeLength, ' ')) + "│" +
                    //                ("".PadRight(nameLength, ' ')) + "│" +
                    //                ("".PadRight(propagationLength, ' ')) + "│");
                    //    }
                    //    else
                    //    {
                    //        for (int i = 0; i < list.Count; i++)
                    //        {
                    //            Printer.Print("│" +
                    //                ((i == 0 ? per.Value.ToString() : "").PadRight(valueLength, ' ')) + "│" +
                    //                ((i == 0 ? per.Function.Name : "").PadRight(functionLength, ' ')) + "│" +
                    //                (list[i].Discriminator.TypeName.PadRight(typeLength, ' ')) + "│" +
                    //                (list[i].Discriminator.Name.PadRight(nameLength, ' ')) + "│" +
                    //                (list[i].Propagation.ToString().PadRight(propagationLength, ' ')) + "│");
                    //        }
                    //    }
                    //}
                    //Printer.Print("└" + ("".PadRight(valueLength, '─')) + "┴" + ("".PadRight(functionLength, '─')) + "┴" + ("".PadRight(typeLength, '─')) + "┴" + ("".PadRight(nameLength, '─')) + "┴" + ("".PadRight(propagationLength, '─')) + "┘");
                    break;
            }
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
            Read = new Function<string>(READ);
            Edit = new Function<string>(EDIT);
            Create = new Function<string>(CREATE);
            Delete = new Function<string>(DELETE);
            Manage = new Function<string>(MANAGE);
            Admin = new Function<string>(ADMIN);
            ((Function<string>)Read).Exclusions = new[] { Edit }.Cast<IFunction<string>>();

            ((Function<string>)Edit).Inclusions = new[] { Read }.Cast<IFunction<string>>();
            ((Function<string>)Edit).Exclusions = new[] { Create, Delete }.Cast<IFunction<string>>();

            ((Function<string>)Create).Inclusions = new[] { Edit }.Cast<IFunction<string>>();
            ((Function<string>)Create).Exclusions = new[] { Manage }.Cast<IFunction<string>>();

            ((Function<string>)Delete).Inclusions = new[] { Edit }.Cast<IFunction<string>>();
            ((Function<string>)Delete).Exclusions = new[] { Manage }.Cast<IFunction<string>>();

            ((Function<string>)Manage).Inclusions = new[] { Create, Delete }.Cast<IFunction<string>>();
            ((Function<string>)Manage).Exclusions = new[] { Admin }.Cast<IFunction<string>>();

            ((Function<string>)Admin).Inclusions = new[] { Manage }.Cast<IFunction<string>>();
            dic = new Dictionary<object, IFunction>
            {
                [((Function<string>)Read).Id] = Read,
                [((Function<string>)Edit).Id] = Edit,
                [((Function<string>)Create).Id] = Create,
                [((Function<string>)Delete).Id] = Delete,
                [((Function<string>)Manage).Id] = Manage,
                [((Function<string>)Admin).Id] = Admin,
            };
            graph.AddEdge(Admin, Manage);
            graph.AddEdge(Manage, Edit);
            graph.AddEdge(Manage, Delete);
            graph.AddEdge(Edit, Read);
            graph.AddEdge(Create, Read);
        }
        static Graph<IFunction> graph = new Graph<IFunction>();
        static Dictionary<object, IFunction> dic;
        public const string READ = nameof(READ);
        public const string EDIT = nameof(EDIT);
        public const string CREATE = nameof(CREATE);
        public const string DELETE = nameof(DELETE);
        public const string MANAGE = nameof(MANAGE);
        public const string ADMIN = nameof(ADMIN);
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
        public static IFunction AddCustom<T>(T id, IEnumerable<IFunction> inclusions = null, IEnumerable<IFunction> exclusions = null)
        {
            var me = new Function<T>(id, inclusions, exclusions);
            if (inclusions != null)
                foreach (var inc in inclusions)
                    ((Function<T>)dic[inc.Id]).Exclusions = ((Function<T>)dic[inc.Id]).Exclusions.Union(new[] { me });
            if (exclusions != null)
                foreach (var exc in exclusions)
                    ((Function<T>)dic[exc.Id]).Inclusions = ((Function<T>)dic[exc.Id]).Inclusions.Union(new[] { me });
            return dic[id] = me;
        }
        [DebuggerDisplay("{" + nameof(Name) + "}")]
        class Function<T> : IFunction<T>
        {
            public Function(T id, IEnumerable<IFunction> inclusions = null, IEnumerable<IFunction> exclusions = null)
            {
                Id = id;
                Name = id.ToString();
                Inclusions = inclusions;
                Exclusions = exclusions;
            }
            public T Id { get; private set; }
            object IFunction.Id { get { return Id; } }
            public string Name { get; private set; }

            public IEnumerable<IFunction> Inclusions { get; internal set; }
            public IEnumerable<IFunction> Exclusions { get; internal set; }
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

        //[DebuggerDisplay("{" + nameof(Name) + "}")]
        //class Function : IFunction<string>
        //{
        //    public Function(string id, IEnumerable<IFunction<string>> inclusions = null, IEnumerable<IFunction<string>> exclusions = null)
        //    {
        //        Id = id;
        //        Name = id;
        //        Inclusions = inclusions;
        //        Exclusions = exclusions;
        //    }
        //    public string Id { get; private set; }
        //    object IFunction.Id { get { return Id; } }
        //    public string Name { get; private set; }

        //    public IEnumerable<IFunction<string>> Inclusions { get; internal set; }
        //    public IEnumerable<IFunction<string>> Exclusions { get; internal set; }
        //    IEnumerable<IFunction> IInclusive<IFunction>.Inclusions
        //    {
        //        get
        //        {
        //            return Inclusions;
        //        }
        //    }
        //    IEnumerable<IFunction> IExclusive<IFunction>.Exclusions
        //    {
        //        get
        //        {
        //            return Exclusions;
        //        }
        //    }
        //}
    }
}
