using Fuxion.Identity.Helpers;

namespace Fuxion.Identity;

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
				var idLength   = me.Select(p => p.Id.ToString()?.Length ?? 0).Union(new[] { "ID".Length }).Max();
				var nameLength = me.Select(p => p.Name.ToString().Length).Union(new[] { "NAME".Length }).Max();

				Printer.WriteLine("┌"
										+ ("".PadRight(idLength,         '─'))
										+ "┬" + ("".PadRight(nameLength, '─'))
										+ "┐");
				if (me.Any())
				{
					Printer.WriteLine("│"
											+ ("ID".PadRight(idLength, ' '))
											+ "│" + ("NAME".PadRight(nameLength, ' '))
											+ "│");
					Printer.WriteLine("├"             + ("".PadRight(idLength,   '─'))
														 + "┼" + ("".PadRight(nameLength, '─'))
														 + "┤");
				}
				foreach (var per in me)
				{
					Printer.WriteLine("│"                                               +
											(per.Id.ToString() ?? "").PadRight(idLength, ' ') + "│" +
											per.Name.PadRight(nameLength, ' ')                + "│" +
											"");
				}
				Printer.WriteLine("└"
										+ ("".PadRight(idLength,         '─'))
										+ "┴" + ("".PadRight(nameLength, '─'))
										+ "┘");
				break;
		}
	}
}