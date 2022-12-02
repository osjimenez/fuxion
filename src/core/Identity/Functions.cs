using System.Diagnostics;
using Fuxion.Math.Graph;

namespace Fuxion.Identity;

public static class Functions
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
	static Functions() => CreatePredefinedFunctions();
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
	static Graph<IFunction> graph = new();
	static readonly Dictionary<object, IFunction> dic = new();
	public static IFunction GetById(string id) => dic[id];
	public static IEnumerable<IFunction> GetAll() => dic.Values;
	public static IFunction CreateCustom<T>(T id, IEnumerable<IFunction>? inclusions = null, IEnumerable<IFunction>? exclusions = null) => new Function<T>(id, inclusions, exclusions);
	public static IFunction AddCustom(IFunction function)
	{
		if (function.Inclusions != null)
			foreach (var inc in function.Inclusions)
				((Function)dic[inc.Id]).Exclusions = new[]
				{
					function
				}.Transform(o =>
				{
					if (dic[inc.Id].Exclusions != null) o = o.Union(dic[inc.Id].Exclusions).Where(_ => _ != null).ToArray();
					return o;
				});
		if (function.Exclusions != null)
			foreach (var exc in function.Exclusions)
				((Function)dic[exc.Id]).Inclusions = new[]
				{
					function
				}.Transform(o =>
				{
					if (dic[exc.Id].Inclusions != null) o = o.Union(dic[exc.Id].Inclusions).Where(_ => _ != null).ToArray();
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
		} catch (GraphCyclicException)
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
				((Function)fun).Inclusions = fun.Inclusions.Except(new[]
				{
					function
				}).ToArray();
				graph.Remove(function);
			}
			if (fun.Exclusions?.Contains(function) ?? false)
			{
				((Function)fun).Exclusions = fun.Exclusions.Except(new[]
				{
					function
				}).ToArray();
				graph.Remove(function);
			}
		}
		return dic.Remove(function.Id);
	}
	public static void Reset(bool createPredefinedFunctions = true)
	{
		dic.Clear();
		graph = new();
		if (createPredefinedFunctions) CreatePredefinedFunctions();
	}

	[DebuggerDisplay("{" + nameof(Name) + "}")]
	abstract class Function : IFunction
	{
		public Function(string name) => Name = name;
		public object Id => GetId();
		public string Name { get; set; }
		public IEnumerable<IFunction> Inclusions { get; internal set; } = new List<IFunction>();
		public IEnumerable<IFunction> Exclusions { get; internal set; } = new List<IFunction>();
		protected abstract object GetId();
		public override string ToString() => Name;
	}

	[DebuggerDisplay("{" + nameof(Name) + "}")]
	class Function<T> : Function, IFunction<T>
	{
		public Function(T id, IEnumerable<IFunction>? inclusions = null, IEnumerable<IFunction>? exclusions = null) : base(id?.ToString()
																																								 ?? throw new ArgumentException("'id' cannot be null", nameof(id)))
		{
			Id = id ?? throw new ArgumentException("'id' cannot be null", nameof(id));
			Inclusions = inclusions ?? new List<IFunction>();
			Exclusions = exclusions ?? new List<IFunction>();
		}
		public new T Id { get; }
		object IFunction.Id => Id!;
		protected override object GetId() => Id!;
	}

	#region Predefined functions
	const string READ = nameof(READ);
	const string EDIT = nameof(EDIT);
	const string CREATE = nameof(CREATE);
	const string DELETE = nameof(DELETE);
	const string MANAGE = nameof(MANAGE);
	const string ADMIN = nameof(ADMIN);
	public static IFunction Read { get; private set; }
	public static IFunction Edit { get; private set; }
	public static IFunction Create { get; private set; }
	public static IFunction Delete { get; private set; }
	public static IFunction Manage { get; private set; }
	public static IFunction Admin { get; private set; }
	static void CreatePredefinedFunctions()
	{
		Read = AddCustom(CreateCustom(READ));
		Edit = AddCustom(CreateCustom(EDIT, new[]
		{
			Read
		}));
		Create = AddCustom(CreateCustom(CREATE, new[]
		{
			Edit
		}));
		Delete = AddCustom(CreateCustom(DELETE, new[]
		{
			Edit
		}));
		Manage = AddCustom(CreateCustom(MANAGE, new[]
		{
			Create, Delete
		}));
		Admin = AddCustom(CreateCustom(ADMIN, new[]
		{
			Manage
		}));
	}
	#endregion
}