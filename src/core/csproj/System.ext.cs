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

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Fuxion.Resources;
using System.Globalization;
using Fuxion.Json;

namespace System
{
	public static partial class Extensions
	{
#if (NET471)
		/// <summary>
		/// Permite una clonación en profundidad de origen. 
		/// </summary>
		/// <param name="origen">Objeto serializable</param>
		/// <exception cref="ArgumentExcepcion">
		/// Se produce cuando el objeto no es serializable.
		/// </exception>
		/// <remarks>Extraido desde 
		/// http://es.debugmodeon.com/articulo/clonar-objetos-de-estructura-compleja
		/// </remarks>
		public static T CloneWithBinary<T>(this T source)
        {
            // Verificamos que sea serializable antes de hacer la copia            
            if (!typeof(T).IsSerializable)
                throw new ArgumentException("La clase " + typeof(T).ToString() + " no es serializable");

            // En caso de ser nulo el objeto, se devuelve tal cual
            if (Object.ReferenceEquals(source, null))
                return default(T);

            //Creamos un stream en memoria
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                try
                {
                    formatter.Serialize(stream, source);
                    stream.Seek(0, SeekOrigin.Begin);
                    //Deserializamos la porcón de memoria en el nuevo objeto                
                    return (T)formatter.Deserialize(stream);
                }
                catch (SerializationException ex)
                { throw new ArgumentException(ex.Message, ex); }
                catch { throw; }
            }
        }
#endif
		#region Disposable
		public static DisposableEnvelope<T> AsDisposable<T>(this T me, Action<T> actionOnDispose = null) { return new DisposableEnvelope<T>(me, actionOnDispose); }
		#endregion
		#region Json
		public static string ToJson(this object me, Formatting formatting = Formatting.Indented, JsonSerializerSettings settings = null)
		{
			if (settings != null)
				return JsonConvert.SerializeObject(me, formatting, settings);
			return JsonConvert.SerializeObject(me, formatting);
		}
		public static string ToJson(this Exception me, Formatting formatting = Formatting.Indented)
		{
			return me.ToJson(
				formatting,
				new JsonSerializerSettings().Transform<JsonSerializerSettings>(s =>
					s.ContractResolver = new ExceptionContractResolver
					{
						IgnoreSerializableInterface = true,
						IgnoreSerializableAttribute = true
					}));
		}
		public static T FromJson<T>(this string me, JsonSerializerSettings settings = null) => JsonConvert.DeserializeObject<T>(me, settings);
		public static object FromJson(this string me, Type type) => JsonConvert.DeserializeObject(me, type);
		public static T CloneWithJson<T>(this T me) => (T)FromJson(me.ToJson(), me.GetType());
		#endregion
		#region Transform
		public static TResult Transform<TSource, TResult>(this TSource me, Func<TSource, TResult> transformFunction)
		{
			return transformFunction(me);
		}
		public static TSource Transform<TSource>(this TSource me, Action<TSource> transformFunction)
		{
			transformFunction(me);
			return me;
		}
		public static IEnumerable<TSource> Transform<TSource>(this IEnumerable<TSource> me, Action<TSource> transformFunction)
		{
			foreach (var item in me)
				transformFunction(item);
			return me;
		}
		public static async Task<IEnumerable<TSource>> Transform<TSource>(this IEnumerable<TSource> me, Func<TSource, Task> transformFunction)
		{
			foreach (var item in me)
				await transformFunction(item);
			return me;
		}
		#endregion
		#region Reflections
		public static bool IsNullable(this Type me, bool valueTypesAreNotNullables = true)
		{
			if (valueTypesAreNotNullables && me == typeof(Enum))
				return false;
			if (me.IsClass || me.IsInterface || me.IsGenericType && me.GetGenericTypeDefinition() == typeof(Nullable<>))
				return true;
			//if (!(valueTypesAreNotNullables && typeof(ValueType).IsAssignableFrom(me)))
			//	return true;
			return false;
		}
		public static bool IsNullableValue<T>(this Type me) where T : struct
			=> me.IsGenericType && me.GetGenericTypeDefinition() == typeof(Nullable<>) && me.GetGenericArguments()[0] == typeof(T);
		public static bool IsNullableEnum(this Type me, bool valueTypesAreNotNullables = true)
			=> me.IsGenericType && me.GetGenericTypeDefinition() == typeof(Nullable<>) && me.GetGenericArguments()[0].IsEnum;
		public static object GetDefaultValue(this Type me)
			=> me.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(me) == null
				? Activator.CreateInstance(me)
				: null;
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
		public static bool IsSubclassOfRawGeneric(this Type me, Type generic) => GetSubclassOfRawGeneric(me, generic) != null;
		public static Type GetSubclassOfRawGeneric(this Type me, Type generic)
		{
			Queue<Type> toProcess = new Queue<Type>(new[] { me });
			while (toProcess.Count > 0)
			{
				var actual = toProcess.Dequeue();
				var cur = actual.GetTypeInfo().IsGenericType ? actual.GetGenericTypeDefinition() : actual;
				if (cur.GetTypeInfo().IsGenericType && generic.GetGenericTypeDefinition() == cur.GetGenericTypeDefinition())
				{
					return actual;
				}
				foreach (var inter in actual.GetTypeInfo().ImplementedInterfaces)
					toProcess.Enqueue(inter);
				if (actual.GetTypeInfo().BaseType != null)
					toProcess.Enqueue(actual.GetTypeInfo().BaseType);
			}
			return null;
		}
		#endregion
		#region TimeSpan
		public static string ToTimeString(this TimeSpan ts, int numberOfElements = 5, bool onlyLetters = false)
		{
			string res = "";
			int count = 0;
			if (count >= numberOfElements) return res.Trim(',', ' ');
			if (ts.Days > 0)
			{
				res += $"{ts.Days} {(onlyLetters ? "d" : (ts.Days > 1 ? Strings.days : Strings.day))}{(onlyLetters ? "" : ",")} ";
				count++;
			}
			if (count >= numberOfElements) return res.Trim(',', ' ');
			if (ts.Hours > 0)
			{
				res += $"{ts.Hours} {(onlyLetters ? "h" : (ts.Hours > 1 ? Strings.hours : Strings.hour))}{(onlyLetters ? "" : ",")} ";
				count++;
			}
			if (count >= numberOfElements) return res.Trim(',', ' ');
			if (ts.Minutes > 0)
			{
				res += $"{ts.Minutes} {(onlyLetters ? "m" : (ts.Minutes > 1 ? Strings.minutes : Strings.minute))}{(onlyLetters ? "" : ",")} ";
				count++;
			}
			if (count >= numberOfElements) return res.Trim(',', ' ');
			if (ts.Seconds > 0)
			{
				res += $"{ts.Seconds} {(onlyLetters ? "s" : (ts.Seconds > 1 ? Strings.seconds : Strings.minute))}{(onlyLetters ? "" : ",")} ";
				count++;
			}
			if (count >= numberOfElements) return res.Trim(',', ' ');
			if (ts.Milliseconds > 0)
			{
				res += $"{ts.Milliseconds} {(onlyLetters ? "ms" : (ts.Milliseconds > 1 ? Strings.milliseconds : Strings.millisecond))}{(onlyLetters ? "" : ",")} ";
				count++;
			}
			return res.Trim(',', ' ');
		}
		#endregion
		#region String
		public static string[] SplitInLines(this string me, bool removeEmptyLines = false, bool trimEachLine = false)
			=> me.Split(new[] { "\r\n", "\r", "\n" }, removeEmptyLines ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);		
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
		public static string RandomString(this string me, int length, Random ran = null)
		{
			const string defaultStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			if (ran == null) ran = new Random((int)DateTime.Now.Ticks);
			var str = string.IsNullOrWhiteSpace(me) ? defaultStr : me;
			return new string(Enumerable.Repeat(str, length)
				.Select(s => s[ran.Next(s.Length)]).ToArray());
		}
		public static string ToTitleCase(this string me, CultureInfo culture = null)
			=> (culture ?? CultureInfo.CurrentCulture).TextInfo.ToTitleCase(me.ToLower());
		public static string ToCamelCase(this string me, CultureInfo culture = null)
			=> me.ToTitleCase(culture).Replace(" ", "").Transform(s => s.Substring(0, 1).ToLower() + s.Substring(1, s.Length - 1));
		public static string ToPascalCase(this string me, CultureInfo culture = null)
			=> me.ToTitleCase(culture).Replace(" ", "");
		#endregion
		#region IsBetween
		public static bool IsBetween(this short me, short minimum, short maximum) => minimum <= me && me <= maximum;
		public static bool IsBetween(this ushort me, ushort minimum, ushort maximum) => minimum <= me && me <= maximum;
		public static bool IsBetween(this int me, int minimum, int maximum) => minimum <= me && me <= maximum;
		public static bool IsBetween(this uint me, uint minimum, uint maximum) => minimum <= me && me <= maximum;
		public static bool IsBetween(this long me, long minimum, long maximum) => minimum <= me && me <= maximum;
		public static bool IsBetween(this ulong me, ulong minimum, ulong maximum) => minimum <= me && me <= maximum;
		public static bool IsBetween(this decimal me, decimal minimum, decimal maximum) => minimum <= me && me <= maximum;
		public static bool IsBetween(this double me, double minimum, double maximum) => minimum <= me && me <= maximum;
		public static bool IsBetween(this float me, float minimum, float maximum) => minimum <= me && me <= maximum;
		public static bool IsBetween(this DateTime me, DateTime minimum, DateTime maximum) => minimum <= me && me <= maximum;
		public static bool IsBetween(this TimeSpan me, TimeSpan minimum, TimeSpan maximum) => minimum <= me && me <= maximum;
		#endregion
		#region IsNullOrDefault
		public static bool IsNullOrDefault<T>(this T me) => EqualityComparer<T>.Default.Equals(me, default(T));
		#endregion
		#region DateTime
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
		#endregion
	}
}