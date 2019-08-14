using Fuxion.Identity.Helpers;
using Fuxion.Math.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
		public static bool IsValid(this IFunction me) => !Comparer.AreEquals(me.Id, me.Id?.GetType().GetDefaultValue()) && !string.IsNullOrWhiteSpace(me.Name);
		private static IEnumerable<T> GetAllInclusions<T>(this IInclusive<T> me, List<T> progress)
		{
			var res = progress;
			if (me.Inclusions == null)
				return res;
			var fs = me.Inclusions.Except(res).ToList();
			res.AddRange(fs);
			foreach (var f in fs.Cast<IInclusive<T>>())
				GetAllInclusions(f, res);
			return res;
		}
		public static IEnumerable<T> GetAllInclusions<T>(this IInclusive<T> me) => GetAllInclusions(me, new List<T>(new T[] { }));
		public static IEnumerable<IDiscriminator> GetAllRelated(this IDiscriminator me, ScopePropagation propagation)
		{
			var res = new List<IDiscriminator>();
			if (propagation.HasFlag(ScopePropagation.ToMe))
				res.Add(me);
			if (propagation.HasFlag(ScopePropagation.ToInclusions))
				res.AddRange(me.GetAllInclusions());
			if (propagation.HasFlag(ScopePropagation.ToExclusions))
				res.AddRange(me.GetAllExclusions());
			return res;
		}
		private static IEnumerable<T> GetAllExclusions<T>(this IExclusive<T> me, List<T> progress)
		{
			var res = progress;
			if (me.Exclusions == null)
				return res;
			var fs = me.Exclusions.Except(res).ToList();
			res.AddRange(fs);
			foreach (var f in fs.Cast<IExclusive<T>>())
				GetAllExclusions(f, res);
			return res;
		}
		public static IEnumerable<T> GetAllExclusions<T>(this IExclusive<T> me) => GetAllExclusions(me, new List<T>(new T[] { }));
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

					Printer.WriteLine("┌"
						+ ("".PadRight(idLength, '─'))
						+ "┬" + ("".PadRight(nameLength, '─'))
						+ "┐");
					if (me.Any())
					{
						Printer.WriteLine("│"
							+ ("ID".PadRight(idLength, ' '))
							+ "│" + ("NAME".PadRight(nameLength, ' '))
							+ "│");
						Printer.WriteLine("├" + ("".PadRight(idLength, '─'))
							+ "┼" + ("".PadRight(nameLength, '─'))
							+ "┤");
					}
					foreach (var per in me)
					{
						Printer.WriteLine("│" +
								per.Id.ToString().PadRight(idLength, ' ') + "│" +
								per.Name.PadRight(nameLength, ' ') + "│" +
								"");
					}
					Printer.WriteLine("└"
						+ ("".PadRight(idLength, '─'))
						+ "┴" + ("".PadRight(nameLength, '─'))
						+ "┘");
					break;
			}
		}
	}

	internal class FunctionEqualityComparer : IEqualityComparer<IFunction>
	{
		public bool Equals(IFunction x, IFunction y) => AreEquals(x, y);

		public int GetHashCode(IFunction obj)
		{
			if (obj == null)
				return 0;
			return obj.Id.GetHashCode();
		}

		private static bool AreEquals(object obj1, object obj2)
		{
			// If both are NULL, return TRUE
			if (Equals(obj1, null) && Equals(obj2, null))
				return true;
			// If some of them is null, return FALSE
			if (Equals(obj1, null) || Equals(obj2, null))
				return false;
			// If any of them are of other type, return FALSE
			if (!(obj1 is IFunction) || !(obj2 is IFunction))
				return false;
			var fun1 = (IFunction)obj1;
			var fun2 = (IFunction)obj2;
			// Compare the ids
			return Comparer.AreEquals(fun1.Id, fun2.Id);
		}
	}
	public static class Functions
	{
		static Functions() => CreatePredefinedFunctions();

		private static Graph<IFunction> graph = new Graph<IFunction>();
		private static Dictionary<object, IFunction> dic = new Dictionary<object, IFunction>();
		#region Predefined functions
		private const string READ = nameof(READ);
		private const string EDIT = nameof(EDIT);
		private const string CREATE = nameof(CREATE);
		private const string DELETE = nameof(DELETE);
		private const string MANAGE = nameof(MANAGE);
		private const string ADMIN = nameof(ADMIN);
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

		public static IFunction CreateCustom<T>(T id, IEnumerable<IFunction>? inclusions = null, IEnumerable<IFunction>? exclusions = null)
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
			foreach (var fun in dic.Values)
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
		public static void Reset(bool createPredefinedFunctions = true)
		{
			dic.Clear();
			graph = new Graph<IFunction>();
			if (createPredefinedFunctions)
				CreatePredefinedFunctions();
		}
		[DebuggerDisplay("{" + nameof(Name) + "}")]
		private abstract class Function : IFunction
		{
			public Function(string name)
			{
				Name = name;
			}
			public object Id => GetId();
			protected abstract object GetId();
			public string Name { get; set; }

			public IEnumerable<IFunction> Inclusions { get; internal set; } = new List<IFunction>();
			public IEnumerable<IFunction> Exclusions { get; internal set; } = new List<IFunction>();

			public override string ToString() => Name;
		}
		[DebuggerDisplay("{" + nameof(Name) + "}")]
		private class Function<T> : Function, IFunction<T>
		{
			public Function(T id, IEnumerable<IFunction>? inclusions = null, IEnumerable<IFunction>? exclusions = null) 
				: base(id?.ToString() ?? throw new ArgumentException("'id' cannot be null", nameof(id)))
			{
				Id = id ?? throw new ArgumentException("'id' cannot be null", nameof(id));
				Inclusions = inclusions ?? new List<IFunction>();
				Exclusions = exclusions ?? new List<IFunction>();
			}
			public new T Id { get; }
			protected override object GetId() => Id!;
			object IFunction.Id => Id!;
		}
	}
}
