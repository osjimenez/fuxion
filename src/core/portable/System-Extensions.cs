using Fuxion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System
{
    public static class Extensions
    {
        #region Disposable
        public static IDisposable AsDisposable<T>(this T me, Action<T> actionOnDispose) { return new DisposableEnvelope<T>(me, actionOnDispose); }
        class DisposableEnvelope<T> : IDisposable
        {
            public DisposableEnvelope(T obj, Action<T> actionOnDispose)
            {
                this.action = actionOnDispose;
                this.obj = obj;
            }
            T obj;
            Action<T> action;
            void IDisposable.Dispose() { action(obj); }
        }
        #endregion
        #region Json
        public static string ToJson(this object me, Formatting formatting = Formatting.Indented, JsonSerializerSettings settings = null)
        {
            if (settings != null)
                return JsonConvert.SerializeObject(me, formatting, settings);
            return JsonConvert.SerializeObject(me, formatting);
        }
        public static T FromJson<T>(this string me, JsonSerializerSettings settings = null) { return (T)JsonConvert.DeserializeObject(me, typeof(T), settings); }
        public static object FromJson(this string me, Type type) { return JsonConvert.DeserializeObject(me, type); }
        public static T CloneWithJson<T>(this T me) { return (T)FromJson(me.ToJson(), me.GetType()); }
        #endregion
        #region Transform
        public static TResult Transform<TSource, TResult>(this TSource me, Func<TSource, TResult> transformFunction)
        {
            return transformFunction(me);
        }
        #endregion
        #region Reflections
        public static string GetFullNameWithAssemblyName(this Type me) { return $"{me.AssemblyQualifiedName.Split(',').Take(2).Aggregate("", (a, n) => a + ", " + n, a => a.Trim(' ', ','))}"; }
        public static string GetSignature(this Type type, bool useFullNames)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null)
                return nullableType.GetSignature(useFullNames) + "?";
            var typeName = useFullNames && !string.IsNullOrWhiteSpace(type.FullName) ? type.FullName : type.Name;
            if (!type.GetTypeInfo().IsGenericType)
                switch (type.Name)
                {
                    case "String": return "string";
                    case "Int32": return "int";
                    case "Int64": return "long";
                    case "Decimal": return "decimal";
                    case "Object": return "object";
                    case "Void": return "void";
                    default:
                        {
                            return typeName;
                        }
                }

            var sb = new StringBuilder(typeName.Substring(0, typeName.IndexOf('`')));
            sb.Append('<');
            var first = true;
            foreach (var t in type.GenericTypeArguments)
            {
                if (!first)
                    sb.Append(',');
                sb.Append(GetSignature(t, useFullNames));
                first = false;
            }
            sb.Append('>');
            return sb.ToString();
        }
        public static bool IsSubclassOfRawGeneric(this Type me, Type generic)
        {
            Queue<Type> toProcess = new Queue<Type>(new[] { me });
            while (toProcess.Count > 0)
            {
                var actual = toProcess.Dequeue();
                var cur = actual.GetTypeInfo().IsGenericType ? actual.GetGenericTypeDefinition() : actual;
                if (cur.GetTypeInfo().IsGenericType && generic.GetGenericTypeDefinition() == cur.GetGenericTypeDefinition())
                {
                    return true;
                }
                foreach (var inter in actual.GetTypeInfo().ImplementedInterfaces)
                    toProcess.Enqueue(inter);
                if (actual.GetTypeInfo().BaseType != null)
                    toProcess.Enqueue(actual.GetTypeInfo().BaseType);
            }
            return false;
        }
        #endregion
        #region TimeSpan
        public static string ToTimeString(this TimeSpan ts)
        {
            string res = "";
            if (ts.Days > 0) res += ts.Days + "d ";
            if (ts.Hours > 0) res += ts.Hours + "h ";
            if (ts.Minutes > 0) res += ts.Minutes + "m ";
            if (ts.Seconds > 0) res += ts.Seconds + "s ";
            if (ts.Milliseconds > 0) res += ts.Milliseconds + "ms ";
            //if (ts.Ticks > 0) res += ts.Ticks + " ticks ";
            res = res.Trim();
            if (string.IsNullOrWhiteSpace(res)) res = "0ms";
            return res;
        }
        #endregion
        #region String
        /// <summary>
        /// Returns true if <paramref name="path"/> starts with the path <paramref name="baseDirPath"/>.
        /// The comparison is case-insensitive, handles / and \ slashes as folder separators and
        /// only matches if the base dir folder name is matched exactly ("c:\foobar\file.txt" is not a sub path of "c:\foo").
        /// </summary>
        public static bool IsSubPathOf(this string path, string baseDirPath)
        {
            string normalizedPath = path.Replace('/', '\\').WithEnding("\\");
            string normalizedBaseDirPath = baseDirPath.Replace('/', '\\').WithEnding("\\");
            return normalizedPath.StartsWith(normalizedBaseDirPath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns <paramref name="str"/> with the minimal concatenation of <paramref name="ending"/> (starting from end) that
        /// results in satisfying .EndsWith(ending).
        /// </summary>
        /// <example>"hel".WithEnding("llo") returns "hello", which is the result of "hel" + "lo".</example>
        public static string WithEnding(this string str, string ending)
        {
            if (str == null)
                return ending;

            string result = str;

            // Right() is 1-indexed, so include these cases
            // * Append no characters
            // * Append up to N characters, where N is ending length
            for (int i = 0; i <= ending.Length; i++)
            {
                string tmp = result + ending.Right(i);
                if (tmp.EndsWith(ending))
                    return tmp;
            }

            return result;
        }

        /// <summary>Gets the rightmost <paramref name="length" /> characters from a string.</summary>
        /// <param name="value">The string to retrieve the substring from.</param>
        /// <param name="length">The number of characters to retrieve.</param>
        /// <returns>The substring.</returns>
        public static string Right(this string value, int length)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", length, "Length is less than zero");
            }

            return (length < value.Length) ? value.Substring(value.Length - length) : value;
        }
        #endregion
    }
    public static class DateTimeExtensions
    {
        public static DateTime AverageDateTime(this IEnumerable<DateTime> me)
        {
            var list = me.ToList();
            double temp = 0D;
            for (int i = 0; i < list.Count; i++)
            {
                temp += list[i].Ticks / (double)list.Count;
            }
            return new DateTime((long)temp);
        }
        public static DateTimeOffset AverageDateTime(this IEnumerable<DateTimeOffset> me)
        {
            var list = me.ToList();
            double temp = 0D;
            for (int i = 0; i < list.Count; i++)
            {
                temp += list[i].Ticks / (double)list.Count;
            }
            return new DateTimeOffset(new DateTime((long)temp));
        }
        // TODO - Remove outliers: http://www.ehow.com/how_5201412_calculate-outliers.html
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
            var getQuartileFunction = new Func<int,double>(q => {
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
