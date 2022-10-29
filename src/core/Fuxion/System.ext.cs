using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fuxion.Json;
using Fuxion.Resources;

namespace System;

public static class Extensions
{
	/// <summary>
	///    Permite una clonación en profundidad de origen.
	/// </summary>
	/// <param name="origen">Objeto serializable</param>
	/// <exception cref="ArgumentExcepcion">
	///    Se produce cuando el objeto no es serializable.
	/// </exception>
	/// <remarks>
	///    Extraido desde
	///    http://es.debugmodeon.com/articulo/clonar-objetos-de-estructura-compleja
	/// </remarks>
	//public static T CloneWithBinary<T>(this T source)
	//      {
	//          // Verificamos que sea serializable antes de hacer la copia            
	//          if (!typeof(T).IsSerializable)
	//              throw new ArgumentException("La clase " + typeof(T).ToString() + " no es serializable");

	//          // En caso de ser nulo el objeto, se devuelve tal cual
	//          if (Object.ReferenceEquals(source, null))
	//              return default!;

	//          //Creamos un stream en memoria
	//          IFormatter formatter = new BinaryFormatter();
	//          Stream stream = new MemoryStream();
	//          using (stream)
	//          {
	//              try
	//              {
	//                  formatter.Serialize(stream, source);
	//                  stream.Seek(0, SeekOrigin.Begin);
	//                  //Deserializamos la porcón de memoria en el nuevo objeto                
	//                  return (T)formatter.Deserialize(stream);
	//              }
	//              catch (SerializationException ex)
	//              { throw new ArgumentException(ex.Message, ex); }
	//              catch { throw; }
	//          }
	//      }

	#region Disposable
	public static DisposableEnvelope<T> AsDisposable<T>(this T me, Action<T>? actionOnDispose = null) where T : notnull => new(me, actionOnDispose);
	#endregion

	#region IsNullOrDefault
	public static bool IsNullOrDefault<T>(this T me) => EqualityComparer<T>.Default.Equals(me, default!);
	#endregion

	#region Json
	public static string ToJson(this object? me, JsonSerializerOptions? options = null) =>
		JsonSerializer.Serialize(me, me?.GetType() ?? typeof(object), options ?? new JsonSerializerOptions
		{
			WriteIndented = true
		});
	public static string ToJson(this Exception me, JsonSerializerOptions? options = null)
	{
		options ??= new()
		{
			WriteIndented = true
		};
		if (!options.Converters.Any(c => c.GetType().IsSubclassOfRawGeneric(typeof(FallbackConverter<>))))
			options.Converters.Add(new FallbackConverter<Exception>(new MultilineStringToCollectionPropertyFallbackResolver()));
		return JsonSerializer.Serialize(me, options);
	}
	public static T? FromJson<T>(this string me, [DoesNotReturnIf(true)] bool exceptionIfNull = false, JsonSerializerOptions? options = null, bool usePrivateConstructor = true) =>
		(T?)FromJson(me, typeof(T), exceptionIfNull, options, usePrivateConstructor);
	public static object? FromJson(this string me, Type type, [DoesNotReturnIf(true)] bool exceptionIfNull = false, JsonSerializerOptions? options = null, bool usePrivateConstructor = true)
	{
		if (usePrivateConstructor)
			options ??= new()
			{
				TypeInfoResolver = new PrivateConstructorContractResolver()
			};
		var res = JsonSerializer.Deserialize(me, type, options);
		if (exceptionIfNull && res is null) throw new SerializationException($"The string cannot be deserialized as '{type.Name}':\r\n{me}");
		return res;
	}
	public static T CloneWithJson<T>(this T me) => (T)(FromJson(me?.ToJson() ?? throw new InvalidDataException(), me?.GetType() ?? throw new InvalidDataException()) ?? default!);
	#endregion

	#region Transform
	public static TResult Transform<TSource, TResult>(this TSource me, Func<TSource, TResult> transformFunction) => transformFunction(me);
	public static TSource Transform<TSource>(this TSource me, Action<TSource> transformFunction)
	{
		transformFunction(me);
		return me;
	}
	public static IEnumerable<TSource> Transform<TSource>(this IEnumerable<TSource> me, Action<TSource> transformFunction)
	{
		foreach (var item in me) transformFunction(item);
		return me;
	}
	public static async Task<IEnumerable<TSource>> Transform<TSource>(this IEnumerable<TSource> me, Func<TSource, Task> transformFunction)
	{
		foreach (var item in me) await transformFunction(item);
		return me;
	}
	public static ICollection<TSource> Transform<TSource>(this ICollection<TSource> me, Action<TSource> transformFunction)
	{
		foreach (var item in me) transformFunction(item);
		return me;
	}
	public static async Task<ICollection<TSource>> Transform<TSource>(this ICollection<TSource> me, Func<TSource, Task> transformFunction)
	{
		foreach (var item in me) await transformFunction(item);
		return me;
	}
	#endregion

	#region Reflections
	public static bool IsNullable(this Type me, bool valueTypesAreNotNullables = true)
	{
		if (valueTypesAreNotNullables && me == typeof(Enum)) return false;
		if (me.IsClass || me.IsInterface || me.IsGenericType && me.GetGenericTypeDefinition() == typeof(Nullable<>)) return true;
		//if (!(valueTypesAreNotNullables && typeof(ValueType).IsAssignableFrom(me)))
		//	return true;
		return false;
	}
	public static bool IsNullableValue<T>(this Type me) where T : struct => me.IsGenericType && me.GetGenericTypeDefinition() == typeof(Nullable<>) && me.GetGenericArguments()[0] == typeof(T);
	public static bool IsNullableEnum(this Type me, bool valueTypesAreNotNullables = true) =>
		me.IsGenericType && me.GetGenericTypeDefinition() == typeof(Nullable<>) && me.GetGenericArguments()[0].IsEnum;
	public static object? GetDefaultValue(this             Type me) => me.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(me) == null ? Activator.CreateInstance(me) : null;
	public static string  GetFullNameWithAssemblyName(this Type me) => $"{me.AssemblyQualifiedName?.Split(',').Take(2).Aggregate("", (a, n) => a + ", " + n, a => a.Trim(' ', ','))}";
	public static string GetSignature(this Type type, bool useFullNames = false)
	{
		var nullableType = Nullable.GetUnderlyingType(type);
		if (nullableType != null) return nullableType.GetSignature(useFullNames) + "?";
		var typeName = useFullNames && !string.IsNullOrWhiteSpace(type.FullName) ? type.FullName : type.Name;
		if (!type.GetTypeInfo().IsGenericType)
			switch (type.Name)
			{
				case "String":  return "string";
				case "Int32":   return "int";
				case "Int64":   return "long";
				case "Decimal": return "decimal";
				case "Object":  return "object";
				case "Void":    return "void";
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
			if (!first) sb.Append(',');
			sb.Append(GetSignature(t, useFullNames));
			first = false;
		}
		sb.Append('>');
		return sb.ToString();
	}
	public static bool IsSubclassOfRawGeneric(this Type me, Type generic) => GetSubclassOfRawGeneric(me, generic) != null;
	public static Type? GetSubclassOfRawGeneric(this Type me, Type generic)
	{
		var toProcess = new Queue<Type>(new[]
		{
			me
		});
		while (toProcess.Count > 0)
		{
			var actual = toProcess.Dequeue();
			var cur    = actual.GetTypeInfo().IsGenericType ? actual.GetGenericTypeDefinition() : actual;
			if (cur.GetTypeInfo().IsGenericType && generic.GetGenericTypeDefinition() == cur.GetGenericTypeDefinition()) return actual;
			foreach (var inter in actual.GetTypeInfo().ImplementedInterfaces) toProcess.Enqueue(inter);
			var baseType = actual.GetTypeInfo().BaseType;
			if (baseType != null) toProcess.Enqueue(baseType);
		}
		return null;
	}
	// Source: https://stackoverflow.com/questions/1565734/is-it-possible-to-set-private-property-via-reflection
	/// <summary>
	///    Returns a _private_ Property Value from a given Object. Uses Reflection.
	///    Throws a ArgumentOutOfRangeException if the Property is not found.
	/// </summary>
	/// <typeparam name="T">Type of the Property</typeparam>
	/// <param name="obj">Object from where the Property Value is returned</param>
	/// <param name="propName">Propertyname as string.</param>
	/// <returns>PropertyValue</returns>
	public static T? GetPrivatePropertyValue<T>(this object obj, string propName)
	{
		if (obj == null) throw new ArgumentNullException(nameof(obj));
		var pi = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		if (pi == null) throw new ArgumentOutOfRangeException(nameof(propName), string.Format("Property {0} was not found in Type {1}", propName, obj.GetType().FullName));
		return (T?)pi.GetValue(obj, null);
	}
	/// <summary>
	///    Returns a private Property Value from a given Object. Uses Reflection.
	///    Throws a ArgumentOutOfRangeException if the Property is not found.
	/// </summary>
	/// <typeparam name="T">Type of the Property</typeparam>
	/// <param name="obj">Object from where the Property Value is returned</param>
	/// <param name="propName">Propertyname as string.</param>
	/// <returns>PropertyValue</returns>
	public static T? GetPrivateFieldValue<T>(this object obj, string propName)
	{
		if (obj == null) throw new ArgumentNullException(nameof(obj));
		var        t  = obj.GetType();
		FieldInfo? fi = null;
		while (fi == null && t != null)
		{
			fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			t  = t.BaseType;
		}
		if (fi == null) throw new ArgumentOutOfRangeException(nameof(propName), string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
		return (T?)fi.GetValue(obj);
	}
	/// <summary>
	///    Sets a _private_ Property Value from a given Object. Uses Reflection.
	///    Throws a ArgumentOutOfRangeException if the Property is not found.
	/// </summary>
	/// <typeparam name="T">Type of the Property</typeparam>
	/// <param name="obj">Object from where the Property Value is set</param>
	/// <param name="propName">Propertyname as string.</param>
	/// <param name="val">Value to set.</param>
	/// <returns>PropertyValue</returns>
	public static void SetPrivatePropertyValue(this object obj, string propName, object? val)
	{
		var t    = obj.GetType();
		var prop = t.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		if (prop is null) throw new ArgumentOutOfRangeException(nameof(propName), string.Format("Property {0} was not found in Type {1}", propName, obj.GetType().FullName));
		prop.DeclaringType?.InvokeMember(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, Type.DefaultBinder, obj, new[]
		{
			val
		});
	}
	/// <summary>
	///    Set a private Property Value on a given Object. Uses Reflection.
	/// </summary>
	/// <typeparam name="T">Type of the Property</typeparam>
	/// <param name="obj">Object from where the Property Value is returned</param>
	/// <param name="propName">Propertyname as string.</param>
	/// <param name="val">the value to set</param>
	/// <exception cref="ArgumentOutOfRangeException">if the Property is not found</exception>
	public static void SetPrivateFieldValue(this object obj, string propName, object? val)
	{
		if (obj == null) throw new ArgumentNullException(nameof(obj));
		var        t  = obj.GetType();
		FieldInfo? fi = null;
		while (fi == null && t != null)
		{
			fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			t  = t.BaseType;
		}
		if (fi == null) throw new ArgumentOutOfRangeException(nameof(propName), string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
		fi.SetValue(obj, val);
	}
	#endregion

	#region Math
	public static double                          Pow(this                  double me, double power)        => Math.Pow(me, power);
	public static long                            Pow(this                  long   me, long   power)        => (long)Math.Pow(me, power);
	public static int                             Pow(this                  int    me, int    power)        => (int)Math.Pow(me,  power);
	public static (long Quotient, long Remainder) Division(this             long   me, long   dividend)     => (me / dividend, me % dividend);
	public static (long Quotient, long Remainder) DivisionByPowerOfTwo(this long   me, ushort numberOfBits) => me.Division(2.Pow(numberOfBits));
	public static (long Quotient, long Remainder) DivisionByPowerOfTwo(this byte[] me, ushort numberOfBits) =>
		BitConverter.ToInt64(me.Concat(Enumerable.Repeat((byte)0, 8 - me.Length)).ToArray(), 0).DivisionByPowerOfTwo(numberOfBits);
	#endregion

	#region Bytes
	public static string ToHexadecimal(this byte[] me, char? separatorChar = null, bool asBigEndian = false)
	{
		string hex;
		if (asBigEndian)
			hex = BitConverter.ToString(me.Reverse().ToArray());
		else
			hex = BitConverter.ToString(me);
		if (separatorChar != null) return hex.Replace('-', separatorChar.Value);
		return hex.Replace("-", string.Empty);
	}
	public static byte[] ToByteArrayFromHexadecimal(this string me, char? separatorChar = null, bool isBigEndian = false)
	{
		if (separatorChar != null) me = me.RemoveChar(separatorChar.Value);
		var NumberChars               = me.Length;
		var bytes                     = new byte[NumberChars / 2];
		if (isBigEndian)
			for (var i = NumberChars; i > 1; i -= 2)
				bytes[(i - 2) / 2] = Convert.ToByte(me.Substring(i - 2, 2), 16);
		else
			for (var i = 0; i < NumberChars; i += 2)
				bytes[i / 2] = Convert.ToByte(me.Substring(i, 2), 16);
		if (isBigEndian) bytes = bytes.Reverse().ToArray();
		return bytes;
	}
	#endregion

	#region String
	public static string RemoveChar(this string me, char c)
	{
		var res = "";
		for (var i = 0; i < me.Length; i++)
			if (me[i] != c)
				res += me[i];
		return res;
	}
	public static string[] SplitInLines(this string me, bool removeEmptyLines = false, bool trimEachLine = false) =>
		me.Split(new[]
		{
			"\r\n", "\r", "\n"
		}, removeEmptyLines ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
	/// <summary>
	///    Returns true if <paramref name="path" /> starts with the path <paramref name="baseDirPath" />.
	///    The comparison is case-insensitive, handles / and \ slashes as folder separators and
	///    only matches if the base dir folder name is matched exactly ("c:\foobar\file.txt" is not a sub path of "c:\foo").
	/// </summary>
	public static bool IsSubPathOf(this string path, string baseDirPath)
	{
		var normalizedPath        = path.Replace('/', '\\').WithEnding("\\");
		var normalizedBaseDirPath = baseDirPath.Replace('/', '\\').WithEnding("\\");
		return normalizedPath.StartsWith(normalizedBaseDirPath, StringComparison.OrdinalIgnoreCase);
	}
	/// <summary>
	///    Returns <paramref name="str" /> with the minimal concatenation of <paramref name="ending" /> (starting from end) that
	///    results in satisfying .EndsWith(ending).
	/// </summary>
	/// <example>"hel".WithEnding("llo") returns "hello", which is the result of "hel" + "lo".</example>
	public static string WithEnding(this string str, string ending)
	{
		if (str == null) return ending;
		var result = str;

		// Right() is 1-indexed, so include these cases
		// * Append no characters
		// * Append up to N characters, where N is ending length
		for (var i = 0; i <= ending.Length; i++)
		{
			var tmp = result + ending.Right(i);
			if (tmp.EndsWith(ending)) return tmp;
		}
		return result;
	}
	/// <summary>Gets the rightmost <paramref name="length" /> characters from a string.</summary>
	/// <param name="value">The string to retrieve the substring from.</param>
	/// <param name="length">The number of characters to retrieve.</param>
	/// <returns>The substring.</returns>
	public static string Right(this string value, int length)
	{
		if (value  == null) throw new ArgumentNullException("value");
		if (length < 0) throw new ArgumentOutOfRangeException("length", length, "Length is less than zero");
		return length < value.Length ? value.Substring(value.Length - length) : value;
	}
	public static string RandomString(this string me, int length, Random? ran = null)
	{
		const string defaultStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		if (ran == null) ran    = new((int)DateTime.Now.Ticks);
		var str                 = string.IsNullOrWhiteSpace(me) ? defaultStr : me;
		return new(Enumerable.Repeat(str, length).Select(s => s[ran!.Next(s.Length)]).ToArray());
	}
	public static string ToTitleCase(this string me, CultureInfo? culture = null) => (culture ?? CultureInfo.CurrentCulture).TextInfo.ToTitleCase(me.ToLower());
	public static string ToCamelCase(this string me, CultureInfo? culture = null) => me.ToTitleCase(culture).Replace(" ", "").Transform(s => s.Substring(0, 1).ToLower() + s.Substring(1, s.Length - 1));
	public static string ToPascalCase(this string me, CultureInfo? culture = null) => me.ToTitleCase(culture).Replace(" ", "");
	public static bool   Contains(this string source, string value, StringComparison comparisonType) => source != null && value != null && source?.IndexOf(value, comparisonType) >= 0;
	public static IEnumerable<int> AllIndexesOf(this string me, string value, StringComparison comparisonType)
	{
		if (string.IsNullOrEmpty(value)) throw new ArgumentException("The string to find may not be empty", "value");
		for (var index = 0;; index += value.Length)
		{
			index = me.IndexOf(value, index, comparisonType);
			if (index == -1) break;
			yield return index;
		}
	}
	public static List<((int ItemIndex, int PositionIndex) Start, (int ItemIndex, int PositionIndex) End)> SearchTextInElements(this string[] me, string text, StringComparison comparisonType)
	{
		// Concateno el texto de todos los elementos
		var allText = me.Aggregate("", (a, c) => a + c);
		// Busco todas las apariciones del texto buscado
		var indexes = allText.AllIndexesOf(text, comparisonType);
		var res     = new List<((int ItemIndex, int PositionIndex) Start, (int ItemIndex, int PositionIndex) End)>();
		foreach (var index in indexes)
		{
			var counter          = 0;
			var startItemIndex   = 0;
			var startIndexInItem = 0;
			for (; startItemIndex < me.Length; startItemIndex++)
			{
				counter += me[startItemIndex].Length;
				if (counter > index)
				{
					startIndexInItem = me[startItemIndex].Length - (counter - index);
					break;
				}
			}
			var endItemIndex   = startItemIndex;
			var endIndexInItem = 0;
			for (; endItemIndex < me.Length; endItemIndex++)
			{
				if (endItemIndex != startItemIndex) counter += me[endItemIndex].Length;
				if (counter >= index + text.Length)
				{
					endIndexInItem = me[endItemIndex].Length - 1 - (counter - (index + text.Length));
					break;
				}
			}
			res.Add(((startItemIndex, startIndexInItem), (endItemIndex, endIndexInItem)));
			Debug.WriteLine("");
		}
		return res;
	}
	#endregion

	#region IsBetween
	public static bool IsBetween(this short    me, short    minimum, short    maximum) => minimum <= me && me <= maximum;
	public static bool IsBetween(this ushort   me, ushort   minimum, ushort   maximum) => minimum <= me && me <= maximum;
	public static bool IsBetween(this int      me, int      minimum, int      maximum) => minimum <= me && me <= maximum;
	public static bool IsBetween(this uint     me, uint     minimum, uint     maximum) => minimum <= me && me <= maximum;
	public static bool IsBetween(this long     me, long     minimum, long     maximum) => minimum <= me && me <= maximum;
	public static bool IsBetween(this ulong    me, ulong    minimum, ulong    maximum) => minimum <= me && me <= maximum;
	public static bool IsBetween(this decimal  me, decimal  minimum, decimal  maximum) => minimum <= me && me <= maximum;
	public static bool IsBetween(this double   me, double   minimum, double   maximum) => minimum <= me && me <= maximum;
	public static bool IsBetween(this float    me, float    minimum, float    maximum) => minimum <= me && me <= maximum;
	public static bool IsBetween(this DateTime me, DateTime minimum, DateTime maximum) => minimum <= me && me <= maximum;
	public static bool IsBetween(this TimeSpan me, TimeSpan minimum, TimeSpan maximum) => minimum <= me && me <= maximum;
	#endregion

	#region Time
	public static string ToTimeString(this TimeSpan ts, int numberOfElements = 5, bool onlyLetters = false)
	{
		var res   = "";
		var count = 0;
		if (count >= numberOfElements) return res.Trim(',', ' ');
		if (ts.Days > 0)
		{
			res += $"{ts.Days} {(onlyLetters ? "d" : ts.Days > 1 ? Strings.days : Strings.day)}{(onlyLetters ? "" : ",")} ";
			count++;
		}
		if (count >= numberOfElements) return res.Trim(',', ' ');
		if (ts.Hours > 0)
		{
			res += $"{ts.Hours} {(onlyLetters ? "h" : ts.Hours > 1 ? Strings.hours : Strings.hour)}{(onlyLetters ? "" : ",")} ";
			count++;
		}
		if (count >= numberOfElements) return res.Trim(',', ' ');
		if (ts.Minutes > 0)
		{
			res += $"{ts.Minutes} {(onlyLetters ? "m" : ts.Minutes > 1 ? Strings.minutes : Strings.minute)}{(onlyLetters ? "" : ",")} ";
			count++;
		}
		if (count >= numberOfElements) return res.Trim(',', ' ');
		if (ts.Seconds > 0)
		{
			res += $"{ts.Seconds} {(onlyLetters ? "s" : ts.Seconds > 1 ? Strings.seconds : Strings.second)}{(onlyLetters ? "" : ",")} ";
			count++;
		}
		if (count >= numberOfElements) return res.Trim(',', ' ');
		if (ts.Milliseconds > 0)
		{
			res += $"{ts.Milliseconds} {(onlyLetters ? "ms" : ts.Milliseconds > 1 ? Strings.milliseconds : Strings.millisecond)}{(onlyLetters ? "" : ",")} ";
			count++;
		}
		return res.Trim(',', ' ');
	}
	public static DateTime AverageDateTime(this IEnumerable<DateTime> me)
	{
		var list                                  = me.ToList();
		var temp                                  = 0D;
		for (var i = 0; i < list.Count; i++) temp += list[i].Ticks / (double)list.Count;
		return new((long)temp);
	}
	public static DateTimeOffset AverageDateTime(this IEnumerable<DateTimeOffset> me)
	{
		var list                                  = me.ToList();
		var temp                                  = 0D;
		for (var i = 0; i < list.Count; i++) temp += list[i].Ticks / (double)list.Count;
		return new(new((long)temp));
	}
	static readonly DateTime startTime = new(1970, 1, 1);
	public static long ToEpoch(this DateTime dt, bool failIfPrior1970 = false)
	{
		var res = (long)(dt - startTime).TotalSeconds;
		if (res < 0)
			if (failIfPrior1970)
				throw new InvalidDataException("DateTime cannot be prior 1/1/1970 to be converted to EPOCH date");
			else
				return 0;
		return res;
	}
	public static DateTime ToEpochDateTime(this long   me) => startTime.AddSeconds(me);
	public static DateTime ToEpochDateTime(this double me) => startTime.AddSeconds(me);
	#endregion
}