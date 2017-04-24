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
        string Name { get; set; }
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
                        Printer.WriteLine(fun.Id.ToString() + "-" + fun.Name);
                    }
                    break;
                case PrintMode.PropertyList:
                    break;
                case PrintMode.Table:
                    var idLength = me.Select(p => p.Id.ToString().Length).Union(new[] { "ID".Length }).Max();
                    var nameLength = me.Select(p => p.Name.ToString().Length).Union(new[] { "NAME".Length }).Max();
                    //var typeLength = new[] { "TYPE".Length }.Concat(me.SelectMany(p => p.Scopes.Select(s => s.Discriminator.TypeName.Length))).Max();
                    //var nameLength = new[] { "NAME".Length }.Concat(me.SelectMany(p => p.Scopes.Select(s => s.Discriminator.Name.Length))).Max();
                    //var propagationLength = new[] { "PROPAGATION".Length }.Concat(me.SelectMany(p => p.Scopes.Select(s => s.Propagation.ToString().Length))).Max();

                    Printer.WriteLine("┌" 
                        + ("".PadRight(idLength, '─')) 
                        + "┬" + ("".PadRight(nameLength, '─')) 
                        //+ "┬" + ("".PadRight(typeLength, '─')) 
                        //+ "┬" + ("".PadRight(nameLength, '─')) 
                        //+ "┬" + ("".PadRight(propagationLength, '─')) 
                        + "┐");
                    if (me.Any())
                    {
                        Printer.WriteLine("│" 
                            + ("ID".PadRight(idLength, ' ')) 
                            + "│" + ("NAME".PadRight(nameLength, ' ')) 
                            //+ "│" + ("TYPE".PadRight(typeLength, ' ')) 
                            //+ "│" + ("NAME".PadRight(nameLength, ' ')) 
                            //+ "│" + ("PROPAGATION".PadRight(propagationLength, ' ')) 
                            + "│");
                        Printer.WriteLine("├" + ("".PadRight(idLength, '─')) 
                            + "┼" + ("".PadRight(nameLength, '─')) 
                            //+ "┼" + ("".PadRight(typeLength, '─'))
                            //+ "┼" + ("".PadRight(nameLength, '─'))
                            //+ "┼" + ("".PadRight(propagationLength, '─'))
                            + "┤");
                    }

                    foreach (var per in me)
                    {
                        //var list = per.Scopes.ToList();
                        //if (list.Count == 0)
                        //{
                            Printer.WriteLine("│" +
                                    per.Id.ToString().PadRight(idLength, ' ') + "│" +
                                    per.Name.PadRight(nameLength, ' ') + "│" + 
                                    //("".PadRight(typeLength, ' ')) + "│" +
                                    //("".PadRight(nameLength, ' ')) + "│" +
                                    //("".PadRight(propagationLength, ' ')) + "│" +
                                    "");
                        //}
                        //else
                        //{
                        //    for (int i = 0; i < list.Count; i++)
                        //    {
                        //        Printer.Print("│" +
                        //            ((i == 0 ? per.Value.ToString() : "").PadRight(valueLength, ' ')) + "│" +
                        //            ((i == 0 ? per.Function.Name : "").PadRight(functionLength, ' ')) + "│" +
                        //            (list[i].Discriminator.TypeName.PadRight(typeLength, ' ')) + "│" +
                        //            (list[i].Discriminator.Name.PadRight(nameLength, ' ')) + "│" +
                        //            (list[i].Propagation.ToString().PadRight(propagationLength, ' ')) + "│");
                        //    }
                        //}
                    }
                    Printer.WriteLine("└"
                        + ("".PadRight(idLength, '─')) 
                        + "┴" + ("".PadRight(nameLength, '─')) 
                        //+ "┴" + ("".PadRight(typeLength, '─'))
                        //+ "┴" + ("".PadRight(nameLength, '─'))
                        //+ "┴" + ("".PadRight(propagationLength, '─')) 
                        + "┘");
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
            CreatePredefinedFunctions();
            //Read = new Function<string>(READ);
            //Edit = new Function<string>(EDIT);
            //Create = new Function<string>(CREATE);
            //Delete = new Function<string>(DELETE);
            //Manage = new Function<string>(MANAGE);
            //Admin = new Function<string>(ADMIN);
            //((Function<string>)Read).Exclusions = new[] { Edit }.Cast<IFunction<string>>();

            //((Function<string>)Edit).Inclusions = new[] { Read }.Cast<IFunction<string>>();
            //((Function<string>)Edit).Exclusions = new[] { Create, Delete }.Cast<IFunction<string>>();

            //((Function<string>)Create).Inclusions = new[] { Edit }.Cast<IFunction<string>>();
            //((Function<string>)Create).Exclusions = new[] { Manage }.Cast<IFunction<string>>();

            //((Function<string>)Delete).Inclusions = new[] { Edit }.Cast<IFunction<string>>();
            //((Function<string>)Delete).Exclusions = new[] { Manage }.Cast<IFunction<string>>();

            //((Function<string>)Manage).Inclusions = new[] { Create, Delete }.Cast<IFunction<string>>();
            //((Function<string>)Manage).Exclusions = new[] { Admin }.Cast<IFunction<string>>();

            //((Function<string>)Admin).Inclusions = new[] { Manage }.Cast<IFunction<string>>();
            //dic = new Dictionary<object, IFunction>
            //{
            //    [((Function<string>)Read).Id] = Read,
            //    [((Function<string>)Edit).Id] = Edit,
            //    [((Function<string>)Create).Id] = Create,
            //    [((Function<string>)Delete).Id] = Delete,
            //    [((Function<string>)Manage).Id] = Manage,
            //    [((Function<string>)Admin).Id] = Admin,
            //};
            //graph.AddEdge(Admin, Manage);
            //graph.AddEdge(Manage, Edit);
            //graph.AddEdge(Manage, Delete);
            //graph.AddEdge(Edit, Read);
            //graph.AddEdge(Create, Read);
        }
        static Graph<IFunction> graph = new Graph<IFunction>();
        static Dictionary<object, IFunction> dic = new Dictionary<object, IFunction>();
        #region Predefined functions
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
        private static void CreatePredefinedFunctions()
        {
            Read = AddCustom(CreateCustom(READ));
            Edit = AddCustom(CreateCustom(EDIT, new[] { Read }));
            Create = AddCustom(CreateCustom(CREATE, new[] { Edit }));
            Delete = AddCustom(CreateCustom(DELETE, new[] { Edit }));
            Manage = AddCustom(CreateCustom(MANAGE, new[] { Create, Delete }));
            Admin = AddCustom(CreateCustom(ADMIN, new[] { Manage }));
        }
        #endregion

        public static IFunction GetById(string id) => dic[id];
        public static IEnumerable<IFunction> GetAll() => dic.Values;

        public static IFunction CreateCustom<T>(T id, IEnumerable<IFunction> inclusions = null, IEnumerable<IFunction> exclusions = null)
            => new Function<T>(id, inclusions, exclusions);
        public static IFunction AddCustom(IFunction function)
        {
            if (function.Inclusions != null)
                foreach (var inc in function.Inclusions)
                    ((Function)dic[inc.Id]).Exclusions = new IFunction[] { function }.Transform(o =>
                    {
                        if (dic[inc.Id].Exclusions != null)
                            o = o.Union(dic[inc.Id].Exclusions).Where(_ => _ != null).ToArray();
                        return o;
                    });
            if (function.Exclusions != null)
                foreach (var exc in function.Exclusions)
                    ((Function)dic[exc.Id]).Inclusions = new IFunction[] { function }.Transform(o =>
                    {
                        if (dic[exc.Id].Inclusions != null)
                            o = o.Union(dic[exc.Id].Inclusions).Where(_ => _ != null).ToArray();
                        return o;
                    });
            try
            {
                if (function.Inclusions != null)
                    foreach (var fun in function.Inclusions)
                        graph.AddEdge(function, fun);
                if (function.Exclusions != null)
                    foreach (var fun in function.Exclusions)
                        graph.AddEdge(fun, function);
            }
            catch (GraphCyclicException)
            {
                Remove(function);
                throw;
            }
            return dic[function.Id] = function;
        }
        public static bool Remove(IFunction function)
        {
            foreach(var fun in dic.Values)
            {
                if (fun.Inclusions?.Contains(function) ?? false)
                {
                    ((Function)fun).Inclusions = fun.Inclusions.Except(new[] { function }).ToArray();
                    graph.Remove(function);
                }
                if (fun.Exclusions?.Contains(function) ?? false)
                {
                    ((Function)fun).Exclusions = fun.Exclusions.Except(new[] { function }).ToArray();
                    graph.Remove(function);
                }
            }
            return dic.Remove(function.Id);
        }
        public static void Reset(bool createPredefinedFunctions = true) {
            dic.Clear();
            graph = new Graph<IFunction>();
            if(createPredefinedFunctions)
                CreatePredefinedFunctions();
        }
        [DebuggerDisplay("{" + nameof(Name) + "}")]
        abstract class Function : IFunction
        {
            public object Id => GetId();
            protected abstract object GetId();
            public string Name { get; set; }

            public IEnumerable<IFunction> Inclusions { get; internal set; }
            public IEnumerable<IFunction> Exclusions { get; internal set; }

            public override string ToString() => Name;
        }
        [DebuggerDisplay("{" + nameof(Name) + "}")]
        class Function<T> : Function, IFunction<T>
        {
            public Function(T id, IEnumerable<IFunction> inclusions = null, IEnumerable<IFunction> exclusions = null)
            {
                Id = id;
                Name = id.ToString();
                Inclusions = inclusions;
                Exclusions = exclusions;
            }
            public new T Id { get; private set; }
            protected override object GetId() => Id;
            object IFunction.Id => Id;
        }
    }
}
