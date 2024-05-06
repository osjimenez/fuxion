using System.Security.Cryptography;

namespace System.Collections.Generic;

public static class Extensions
{
	public static List<T> TakeRandomly<T>(this IEnumerable<T> me, int count)
	{
		var list = me.ToList();
		// TODO Implements canRepeat behavior
		//if (!canRepeat && count > list.Count) throw new("'count' cannot be higher than number of elements if 'canRepeat' is false");
		if (count > list.Count) throw new("'count' cannot be higher than number of elements");
		var used = new List<int>();
		List<T> res = new();
		for (var i = 0; i < count; i++)
		{
#if NETSTANDARD2_0 || NET462
			var ran = new Random(Guid.NewGuid().GetHashCode());
			var actual = ran.Next(0, list.Count);
			while (used.Contains(actual)) actual = ran.Next(0, list.Count);
#else
			var actual = RandomNumberGenerator.GetInt32(0,list.Count);
			while (used.Contains(actual)) actual = RandomNumberGenerator.GetInt32(0,list.Count);
#endif
			used.Add(actual);
			res.Add(list[actual]);
		}
		return res;
	}
	public static bool IsNullOrEmpty<T>(this IEnumerable<T> me) => me == null || !me.Any();
	// TODO - Eliminado
	//public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T?> me) where T : class => me.Where(i => i != null).Cast<T>();
	public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T?> me) where T : class => me.OfType<T>();
	public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T?> me) where T : struct => me.Where(i => i != null).Cast<T>();
	public static IQueryable<T> RemoveNulls<T>(this IQueryable<T?> me) where T : class => me.Where(i => i != null).Cast<T>();
	public static IQueryable<T> RemoveNulls<T>(this IQueryable<T?> me) where T : struct => me.Where(i => i != null).Cast<T>();
	public static ICollection<T> RemoveNulls<T>(this ICollection<T?> me) where T : class => me.Where(i => i != null).Cast<T>().ToList();
	public static ICollection<T> RemoveNulls<T>(this ICollection<T?> me) where T : struct => me.Where(i => i != null).Cast<T>().ToList();
	public static T[] RemoveNulls<T>(this T?[] me) where T : class => me.Where(i => i != null).Cast<T>().ToArray();
	public static IList<T> RemoveIf<T>(this IList<T> me, Func<T, bool> predicate)
	{
		var res = new List<T>();
		for (var i = 0; i < me.Count; i++)
		{
			var item = me.ElementAt(i);
			if (predicate(item))
			{
				me.RemoveAt(i);
				res.Add(item);
				i--;
			}
		}
		return res;
	}

	// Remove outliers: http://www.ehow.com/how_5201412_calculate-outliers.html
	public static IEnumerable<int> RemoveOutliers(this IEnumerable<int> list, Action<string>? outputConsole = null) =>
		list.Select(i => (long)i).RemoveOutliers(outputConsole: outputConsole).Select(i => (int)i);
	public static IEnumerable<DateTime> RemoveOutliers(this IEnumerable<DateTime> list, Action<string>? outputConsole = null) =>
		list.Select(i => i.Ticks).RemoveOutliers(outputConsole: outputConsole).Select(t => new DateTime(t));
	public static IEnumerable<long> RemoveOutliers(this IEnumerable<long> me, double interquartileOutlierValueRangeFactor = 1.5, Action<string>? outputConsole = null)
	{
		if (!me.Any()) return me;
		// Sort data in ascending
		var l = me.OrderBy(_ => _).ToList();
		// Calculate median
		double median;
		if (l.Count % 2 == 0) // if even number of elements, average two in the middle
			median = l.Skip(l.Count / 2 - 1).Take(2).Average();
		else // if odd number of elements, take center
			median = l.Skip(l.Count / 2).First();
		// Find the upper quartile Q2
		// http://estadisticapasoapaso.blogspot.com.es/2011/09/los-cuartiles.html
		// Qk = k (N/4)
		// q1 = 1 (N/4)
		// q2 = 2 (N/4)
		var getQuartileFunction = new Func<int, double>(q => {
			outputConsole?.Invoke("Calculating Q" + q);
			var exactPosition = q * ((double)l.Count / 4);
			outputConsole?.Invoke($"   {nameof(exactPosition)} = {exactPosition}");
			var integerPosition = (int)exactPosition - 1;
			if (integerPosition < 0) integerPosition = 0;
			outputConsole?.Invoke($"   {nameof(integerPosition)} = {integerPosition}");
			var restPosition = exactPosition % 1;
			if (restPosition > 0 && integerPosition + 1 == l.Count) restPosition = 0;
			outputConsole?.Invoke($"   {nameof(restPosition)} = {restPosition}");
			var result = (double)l[integerPosition];
			outputConsole?.Invoke($"   {nameof(result)} (before rest) = {(long)result}");
			if (restPosition > 0) result += restPosition * (l[integerPosition + 1] - l[integerPosition]);
			outputConsole?.Invoke($"   {nameof(result)} = {(long)result}");
			return result;
		});
		double firstQuartilePossition = 1 * (l.Count / 4);
		var q1 = getQuartileFunction(1);
		var q2 = getQuartileFunction(2);
		var q3 = getQuartileFunction(3);
		var q4 = getQuartileFunction(4);
		var iq = q3 - q1;
		var mildOutlierRange = iq * interquartileOutlierValueRangeFactor;
		var upperMildOutlierValue = q3 + mildOutlierRange;
		var lowerMildOutlierValue = q1 - mildOutlierRange;
		//var extremeOutlierRange = iq * 3;
		//var upperExtremeOutlierValue = q3 + extremeOutlierRange;
		//var lowerExtremeOutlierValue = q1 - extremeOutlierRange;
		outputConsole?.Invoke("Original values:");
		foreach (var i in l) outputConsole?.Invoke("  - " + i);
		outputConsole?.Invoke("");
		outputConsole?.Invoke("Q1 => " + (long)q1);
		outputConsole?.Invoke("Q2 => " + (long)q2);
		outputConsole?.Invoke("Q3 => " + (long)q3);
		outputConsole?.Invoke("Q4 => " + (long)q4);
		outputConsole?.Invoke("");
		outputConsole?.Invoke("Interquartile range: " + iq);
		outputConsole?.Invoke("interquartileOutlierValueRangeFactor: " + interquartileOutlierValueRangeFactor);
		outputConsole?.Invoke("Mild outlier range: " + mildOutlierRange);
		//outputConsole?.Invoke("Extreme outlier range: " + extremeOutlierRange);
		outputConsole?.Invoke("Upper mild outlier limit: " + (long)upperMildOutlierValue);
		outputConsole?.Invoke("Lower mild outlier limit: " + (long)lowerMildOutlierValue);
		//outputConsole?.Invoke("Upper extreme outlier limit: " + (long)upperExtremeOutlierValue);
		//outputConsole?.Invoke("Lower extreme outlier limit: " + (long)lowerExtremeOutlierValue);
		outputConsole?.Invoke("");
		var res = l.Where(v => v <= upperMildOutlierValue && v >= lowerMildOutlierValue).ToList();
		var outliers = l.Where(v => v > upperMildOutlierValue || v < lowerMildOutlierValue).ToList();
		outputConsole?.Invoke("Outliers:");
		foreach (var i in outliers) outputConsole?.Invoke("  - " + i);
		outputConsole?.Invoke("");
		outputConsole?.Invoke("Result values:");
		foreach (var i in res) outputConsole?.Invoke("  - " + i);
		return res;
	}
	public static IEnumerable<(double Percentage, int Rounded, double Exact)> DistributeAsPercentages(this IEnumerable<double> percentages, int amountOfItems)
	{
		if (percentages.Count() > amountOfItems) throw new InvalidDataException($"{nameof(percentages)}.Count ({percentages.Count()}) must be less than {nameof(amountOfItems)} ({amountOfItems})");
		if (percentages.Sum() != 100) throw new InvalidProgramException($"Percentajes must sum 100, but sum {percentages.Sum()}");
		var ordered = percentages.OrderBy(_ => _);
		var quantities = ordered.Select(value => new {
			Percentage = value,
			Rounded = (int)Math.Floor(amountOfItems * (value / 100d)),
			Exact = amountOfItems * (value / 100d)
		}).ToList();
		quantities = quantities.Select(_ => _ with {
			Rounded = _.Rounded == 0
				? 1
				: _.Rounded
		}).ToList();
		while (quantities.Sum(_ => _.Rounded) > amountOfItems)
		{
			var quantity = quantities.MaxBy(_ => _.Rounded);
			if (quantity is null) throw new InvalidProgramException($"{nameof(quantity)} cannot be null");
			var index = quantities.IndexOf(quantity);
			quantities.Remove(quantity);
			quantities.Insert(index, quantity with { Rounded = quantity.Rounded - 1 });
		}
		return quantities.Select(_ => (_.Percentage, _.Rounded, _.Exact));
	}
	public static IDictionary<string, (double Percentage, int Rounded, double Exact)> DistributeAsPercentages(
		this IList<(string Label, double Percentage)> percentages,
		int amountOfItems)
	{
		if (percentages.Count > amountOfItems)
			throw new InvalidDataException(
				$"{nameof(percentages)}.Count ({percentages.Count}) must be less than {nameof(amountOfItems)} ({amountOfItems})");
		if (percentages.Sum(x => x.Percentage) != 100)
			throw new InvalidProgramException($"Percentages must sum 100, but sum {percentages.Sum(x => x.Percentage)}");
		var ordered = percentages.OrderBy(x => x.Percentage);
		var quantities = ordered.Select(value => new
		{
			value.Label,
			value.Percentage,
			Rounded = (int)Math.Floor(amountOfItems * (value.Percentage / 100d)),
			Exact = amountOfItems * (value.Percentage / 100d)
		}).ToList();
		quantities = quantities.Select(x => x with
		{
			Rounded = x.Rounded == 0
				? 1
				: x.Rounded
		}).ToList();
		while (quantities.Sum(x => x.Rounded) > amountOfItems)
		{
			var quantity = quantities.OrderByDescending(x => x.Rounded).MaxBy(y => y.Exact);
			if (quantity is null) throw new InvalidProgramException($"{nameof(quantity)} cannot be null");
			var index = quantities.IndexOf(quantity);
			quantities.Remove(quantity);
			quantities.Insert(index, quantity with { Rounded = quantity.Rounded - 1 });
		}
		
		while (quantities.Sum(x => x.Rounded) < amountOfItems)
		{
			var quantity = quantities.OrderBy(x => x.Rounded).MaxBy(y => y.Exact);
			if (quantity is null) throw new InvalidProgramException($"{nameof(quantity)} cannot be null");
			var index = quantities.IndexOf(quantity);
			quantities.Remove(quantity);
			quantities.Insert(index, quantity with { Rounded = quantity.Rounded + 1 });
		}
		
		return quantities.Select(x => (x.Label, x.Percentage, x.Rounded, x.Exact))
			.ToDictionary(x => x.Label, x => (x.Percentage, x.Rounded, x.Exact));
	}
}