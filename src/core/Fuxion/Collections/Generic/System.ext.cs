using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
	public static class Extensions
	{
		public static IEnumerable<T> TakeRandomly<T>(this IEnumerable<T> me, int count, Random ran = null, bool canRepeat = false)
		{
			if (ran == null) ran = new Random((int)DateTime.Now.Ticks);
			var list = me.ToList();
			if (!canRepeat && count > list.Count)
				throw new Exception("'count' cannot be higher than number of elements if 'canRepeat' is false");
			var used = new List<int>();
			for (int i = 0; i < count; i++)
			{
				var actual = ran.Next(0, list.Count - 1);
				while (used.Contains(actual))
					actual = ran.Next(0, list.Count);
				used.Add(actual);
				yield return list[actual];
			}
		}
		public static bool IsNullOrEmpty<T>(this IEnumerable<T> me) => me == null || !me.Any();
		public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T> me) => me.Where(i => i != null);
		public static IQueryable<T> RemoveNulls<T>(this IQueryable<T> me) => me.Where(i => i != null);
		public static ICollection<T> RemoveNulls<T>(this ICollection<T> me) => me.Where(i => i != null).ToList();
		public static T[] RemoveNulls<T>(this T[] me) => me.Where(i => i != null).ToArray();
		public static IList<T> RemoveIf<T>(this IList<T> me, Func<T, bool> predicate)
		{
			var res = new List<T>();
			for (int i = 0; i < me.Count; i++)
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
		public static IEnumerable<int> RemoveOutliers(this IEnumerable<int> list, Action<string> outputConsole = null)
		{
			return list.Select(i => (long)i).RemoveOutliers(outputConsole: outputConsole).Select(i => (int)i);
		}
		public static IEnumerable<DateTime> RemoveOutliers(this IEnumerable<DateTime> list, Action<string> outputConsole = null)
		{
			return list.Select(i => i.Ticks).RemoveOutliers(outputConsole: outputConsole).Select(t => new DateTime(t));
		}
		public static IEnumerable<long> RemoveOutliers(this IEnumerable<long> me, double interquartileOutlierValueRangeFactor = 1.5, Action<string> outputConsole = null)
		{
			if (!me.Any()) return me;
			// Sort data in ascending
			var l = me.OrderBy(_ => _).ToList();
			// Calculate median
			double median;
			if (l.Count % 2 == 0) // if even number of elements, average two in the middle
				median = l.Skip((l.Count / 2) - 1).Take(2).Average();
			else // if odd number of elements, take center
				median = l.Skip(l.Count / 2).First();
			// Find the upper quartile Q2
			// http://estadisticapasoapaso.blogspot.com.es/2011/09/los-cuartiles.html
			// Qk = k (N/4)
			// q1 = 1 (N/4)
			// q2 = 2 (N/4)
			var getQuartileFunction = new Func<int, double>(q =>
			{
				outputConsole?.Invoke("Calculating Q" + q);
				var exactPosition = q * ((double)l.Count / 4);
				outputConsole?.Invoke($"   {nameof(exactPosition)} = {exactPosition}");
				var integerPosition = ((int)exactPosition) - 1;
				if (integerPosition < 0) integerPosition = 0;
				outputConsole?.Invoke($"   {nameof(integerPosition)} = {integerPosition}");
				var restPosition = exactPosition % 1;
				if (restPosition > 0 && integerPosition + 1 == l.Count)
					restPosition = 0;
				outputConsole?.Invoke($"   {nameof(restPosition)} = {restPosition}");
				var result = (double)l[integerPosition];
				outputConsole?.Invoke($"   {nameof(result)} (before rest) = {(long)result}");
				if (restPosition > 0) result += (restPosition * (l[integerPosition + 1] - l[integerPosition]));
				outputConsole?.Invoke($"   {nameof(result)} = {(long)result}");
				return result;
			});

			double firstQuartilePossition = 1 * (l.Count / 4);
			double q1 = getQuartileFunction(1);
			double q2 = getQuartileFunction(2);
			double q3 = getQuartileFunction(3);
			double q4 = getQuartileFunction(4);

			var iq = q3 - q1;
			var mildOutlierRange = iq * interquartileOutlierValueRangeFactor;
			var upperMildOutlierValue = q3 + mildOutlierRange;
			var lowerMildOutlierValue = q1 - mildOutlierRange;
			//var extremeOutlierRange = iq * 3;
			//var upperExtremeOutlierValue = q3 + extremeOutlierRange;
			//var lowerExtremeOutlierValue = q1 - extremeOutlierRange;



			outputConsole?.Invoke("Original values:");
			foreach (var i in l)
				outputConsole?.Invoke("  - " + i);
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
			foreach (var i in outliers)
				outputConsole?.Invoke("  - " + i);
			outputConsole?.Invoke("");
			outputConsole?.Invoke("Result values:");
			foreach (var i in res)
				outputConsole?.Invoke("  - " + i);

			return res;
		}
	}
}
