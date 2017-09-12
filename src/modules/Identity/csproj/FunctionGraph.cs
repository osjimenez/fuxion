//using System;
//using System.Collections.Generic;
//using Fuxion.Identity.Helpers;
//using System.Diagnostics;
//using Fuxion.Math.Graph;
//using System.Linq;

//namespace Fuxion.Identity
//{
//    public interface IFunctionGraph
//    {
//        IEnumerable<IFunction> GetIncludedBy(IFunction function);
//        IEnumerable<IFunction> GetExcludedBy(IFunction function);
//        void Add(IFunction function, IFunction impliedFunction);
//    }
//    public class FunctionGraph<TId> : IFunctionGraph
//    {
//        Graph<IFunction> gra = new Graph<IFunction>();
//        List<IFunction> list = new List<IFunction>();

//        public IEnumerable<IFunction> GetIncludedBy(IFunction function) { return gra.GetDescendants(function, new FunctionEqualityComparer()); }
//        public IEnumerable<IFunction> GetExcludedBy(IFunction function) { return gra.GetAscendants(function, new FunctionEqualityComparer()); }

//        public IFunction Find(TId key) { return list.Find(f => Comparer.AreEquals(((TId)f.Id), key)); }
//        public void Add(IFunction function, IFunction impliedFunction)
//        {
//            if (!list.Contains(function)) list.Add(function);
//            if (!list.Contains(impliedFunction)) list.Add(impliedFunction);
//            gra.AddEdge(function, impliedFunction);
//        }
//    }

















//    public class StringFunctionGraph : FunctionGraph<string>
//    {
//        public static IFunction<string> Read = new Function("Read");
//        public static IFunction<string> Edit = new Function("Edit");
//        public static IFunction<string> Create = new Function("Create");
//        public static IFunction<string> Delete = new Function("Delete");
//        public static IFunction<string> Manage = new Function("Manage");
//        public static IFunction<string> Admin = new Function("Admin");
//        public StringFunctionGraph(bool loadDefaults = true)
//        {
//            if (loadDefaults)
//            {
//                Add(Admin, Manage);
//                Add(Manage, Create);
//                Add(Manage, Delete);
//                Add(Create, Edit);
//                Add(Delete, Edit);
//                Add(Edit, Read);
//            }
//        }
//        [DebuggerDisplay("{" + nameof(Name) + "}")]
//        class Function : IFunction<string>
//        {
//            public Function(string id)
//            {
//                Id = id;
//                Name = id;
//            }
//            public string Id { get; private set; }
//            object IFunction.Id { get { return Id; } }
//            public string Name { get; private set; }

//            public IEnumerable<string> Inclusions { get; private set; }
//            public IEnumerable<string> Exclusions { get; private set; }
//            IEnumerable<object> IFunction.Inclusions { get { return Inclusions.Cast<object>(); } }
//            IEnumerable<object> IFunction.Exclusions { get { return Exclusions.Cast<object>(); } }
//        }
//    }


//    public class GuidFunctionGraph : FunctionGraph<Guid>
//    {
//        public static IFunction<Guid> Read = new Function(Guid.Parse("{DDCC7617-9862-4878-AA46-66E66D6E2407}"), "Read");
//        public static IFunction<Guid> Edit = new Function(Guid.Parse("{50CB54A0-5C50-4CDC-8CDA-A0CDF029EEBC}"), "Edit");
//        public static IFunction<Guid> Create = new Function(Guid.Parse("{1B89268B-8334-4514-A404-EAE4834DEA1E}"), "Create");
//        public static IFunction<Guid> Delete = new Function(Guid.Parse("{10253B59-2C93-432B-828C-2CD1F3B0ADE1}"), "Delete");
//        public static IFunction<Guid> Manage = new Function(Guid.Parse("{F739F6AD-8D82-4DF9-AE18-620824043ADF}"), "Manage");
//        public static IFunction<Guid> Admin = new Function(Guid.Parse("{AB0D0204-366F-447A-B8F5-337F165BD152}"), "Admin");
//        public GuidFunctionGraph(bool loadDefaults = true)
//        {
//            if (loadDefaults)
//            {
//                Add(Admin, Manage);
//                Add(Manage, Create);
//                Add(Manage, Delete);
//                Add(Create, Edit);
//                Add(Delete, Edit);
//                Add(Edit, Read);
//            }
//        }
//        [DebuggerDisplay("{" + nameof(Name) + "}")]
//        class Function : IFunction<Guid>
//        {
//            public Function(Guid id, string name)
//            {
//                Id = id;
//                Name = name;
//            }
//            public Guid Id { get; private set; }
//            object IFunction.Id { get { return Id; } }
//            public string Name { get; private set; }

//            public IEnumerable<Guid> Inclusions { get; private set; }
//            public IEnumerable<Guid> Exclusions { get; private set; }
//            IEnumerable<object> IFunction.Inclusions { get { return Inclusions.Cast<object>(); } }
//            IEnumerable<object> IFunction.Exclusions { get { return Exclusions.Cast<object>(); } }
//        }
//    }
//    public static class FunctionCollectionExtensions
//    {
//        public static FunctionCollectionFluent ForFunction(this IFunctionGraph me, IFunction function) { return new FunctionCollectionFluent(me, function); }
//    }
//    public sealed class FunctionCollectionFluent
//    {
//        internal FunctionCollectionFluent(IFunctionGraph collection, IFunction function)
//        {
//            Collection = collection;
//            Function = function;
//        }
//        internal IFunctionGraph Collection { get; set; }
//        internal IFunction Function { get; set; }
//        public void Include(IFunction function) { Collection.Add(Function, function); }
//        public void Exclude(IFunction function) { Collection.Add(function, Function); }
//    }
//}
