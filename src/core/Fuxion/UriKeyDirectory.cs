using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;

namespace Fuxion;

public interface IUriKeyResolver
{
	Type this[UriKey key] { get; }
	UriKey this[Type type] { get; }
	bool ContainsKey(UriKey key);
	bool ContainsKey(Type type);
}
public class UriKeyDirectory : IUriKeyResolver
{
	public UriKeyDirectory()
	{
		SystemRegister = new(this);
	}
	readonly Dictionary<UriKey, Type> _keyToTypeDictionary = new();
	readonly Dictionary<Type, UriKey> _typeToKeyDictionary = new(ReferenceEqualityComparer.Instance);
	public Type this[UriKey key]
	{
		get
		{
			if (_keyToTypeDictionary.TryGetValue(key, out var value)) return value;
			throw new UriKeyNotFoundException($"Key '{key}' not found in '{nameof(UriKeyDirectory)}'");
		}
	}
	public UriKey this[Type type]
	{
		get
		{
			if (_typeToKeyDictionary.TryGetValue(type, out var value)) return value;
			throw new UriKeyNotFoundException($"Type '{type}' not found in '{nameof(UriKeyDirectory)}'");
		}
	}
	public bool ContainsKey(UriKey key) => _keyToTypeDictionary.ContainsKey(key);
	public bool ContainsKey(Type type) => _typeToKeyDictionary.ContainsKey(type);
	public void RegisterAssemblyOf(Type type, Func<(Type Type, UriKeyAttribute? Attribute), bool>? predicate = null) =>
		RegisterAssembly(type.Assembly, predicate);
	public void RegisterAssemblyOf<T>(Func<(Type Type, UriKeyAttribute? Attribute), bool>? predicate = null) =>
		RegisterAssembly(typeof(T).Assembly, predicate);
	public void RegisterAssembly(Assembly assembly, Func<(Type Type, UriKeyAttribute? Attribute), bool>? predicate = null)
	{
		var query = assembly.GetTypes().Where(t => t.HasCustomAttribute<UriKeyAttribute>(false))
			.Select(t => (Type: t, Attribute: t.GetCustomAttribute<UriKeyAttribute>()));
		if (predicate != null) query = query.Where(predicate);
		foreach (var tup in query) Register(tup.Type);
	}
	public void Register<T>() => Register(typeof(T));
	public void Register(Type type)
	{
		var key = type.GetUriKey()
			?? throw new ArgumentException($"The type '{type.Name}' isn't decorated with '{nameof(UriKeyAttribute)}' attribute");
		Register(type, key);
	}
	public void Register<T>(UriKey key) => Register(typeof(T), key);
	public void Register(Type type, UriKey key)
	{
		_keyToTypeDictionary.Add(key, type);
		_typeToKeyDictionary.Add(type, key);
	}
	#region SystemRegister
	public SystemRegistrator SystemRegister { get; }
	public class SystemRegistrator(UriKeyDirectory directory)
	{
		public void All()
		{
			// Built-in types
			Bool();
			BoolArray();
			Byte();
			ByteArray();
			SByte();
			SByteArray();
			SByte();
			Char();
			CharArray();
			Decimal();
			DecimalArray();
			Double();
			DoubleArray();
			Float();
			FloatArray();
			Int();
			IntArray();
			UInt();
			UIntArray();
			NInt();
			NIntArray();
			NUInt();
			NUIntArray();
			Long();
			LongArray();
			ULong();
			ULongArray();
			Short();
			ShortArray();
			UShort();
			UShortArray();
			Object();
			ObjectArray();
			String();
			StringArray();
			Dynamic();
			DynamicArray();
			// Additional types
			DateTime();
			DateTimeArray();
			DateOnly();
			DateOnlyArray();
			TimeOnly();
			TimeOnlyArray();
			JsonNode();
		}
		// Built-in types
		// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/built-in-types
		#region bool
		public static UriKey BoolUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "bool/1.0.0");
		public void Bool() => directory.Register<bool>(BoolUriKey);
		public static UriKey BoolArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "bool[]/1.0.0");
		public void BoolArray() => directory.Register<bool[]>(BoolArrayUriKey);
		#endregion
		#region byte
		public static UriKey ByteUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "byte/1.0.0");
		public void Byte() => directory.Register<byte>(ByteUriKey);
		public static UriKey ByteArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "byte[]/1.0.0");
		public void ByteArray() => directory.Register<byte[]>(ByteArrayUriKey);
		public static UriKey SByteUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "sbyte/1.0.0");
		public void SByte() => directory.Register<sbyte>(SByteUriKey);
		public static UriKey SByteArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "sbyte[]/1.0.0");
		public void SByteArray() => directory.Register<sbyte[]>(SByteArrayUriKey);
		#endregion
		#region char
		public static UriKey CharUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "char/1.0.0");
		public void Char() => directory.Register<char>(CharUriKey);
		public static UriKey CharArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "char[]/1.0.0");
		public void CharArray() => directory.Register<char[]>(CharArrayUriKey);
		#endregion
		#region decimal
		public static UriKey DecimalUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "decimal/1.0.0");
		public void Decimal() => directory.Register<decimal>(DecimalUriKey);
		public static UriKey DecimalArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "decimal[]/1.0.0");
		public void DecimalArray() => directory.Register<decimal[]>(DecimalArrayUriKey);
		#endregion
		#region double
		public static UriKey DoubleUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "double/1.0.0");
		public void Double() => directory.Register<double>(DoubleUriKey);
		public static UriKey DoubleArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "double[]/1.0.0");
		public void DoubleArray() => directory.Register<double[]>(DoubleArrayUriKey);
		#endregion
		#region float
		public static UriKey FloatUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "float/1.0.0");
		public void Float() => directory.Register<float>(FloatUriKey);
		public static UriKey FloatArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "float[]/1.0.0");
		public void FloatArray() => directory.Register<float[]>(FloatArrayUriKey);
		#endregion
		#region int
		public static UriKey IntUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "int/1.0.0");
		public void Int() => directory.Register<int>(IntUriKey);
		public static UriKey IntArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "int[]/1.0.0");
		public void IntArray() => directory.Register<int[]>(IntArrayUriKey);
		public static UriKey UIntUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "uint/1.0.0");
		public void UInt() => directory.Register<uint>(UIntUriKey);
		public static UriKey UIntArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "uint[]/1.0.0");
		public void UIntArray() => directory.Register<uint[]>(UIntArrayUriKey);
		public static UriKey NIntUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "nint/1.0.0");
		public void NInt() => directory.Register<nint>(NIntUriKey);
		public static UriKey NIntArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "nint[]/1.0.0");
		public void NIntArray() => directory.Register<nint[]>(NIntArrayUriKey);
		public static UriKey NUIntUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "nuint/1.0.0");
		public void NUInt() => directory.Register<nuint>(NUIntUriKey);
		public static UriKey NUIntArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "nuint[]/1.0.0");
		public void NUIntArray() => directory.Register<nuint[]>(NUIntArrayUriKey);
		#endregion
		#region long
		public static UriKey LongUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "long/1.0.0");
		public void Long() => directory.Register<long>(LongUriKey);
		public static UriKey LongArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "long[]/1.0.0");
		public void LongArray() => directory.Register<long[]>(LongArrayUriKey);
		public static UriKey ULongUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "ulong/1.0.0");
		public void ULong() => directory.Register<ulong>(ULongUriKey);
		public static UriKey ULongArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "ulong[]/1.0.0");
		public void ULongArray() => directory.Register<ulong[]>(ULongArrayUriKey);
		#endregion
		#region short
		public static UriKey ShortUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "short/1.0.0");
		public void Short() => directory.Register<short>(ShortUriKey);
		public static UriKey ShortArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "short[]/1.0.0");
		public void ShortArray() => directory.Register<short[]>(ShortArrayUriKey);
		public static UriKey UShortUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "ushort/1.0.0");
		public void UShort() => directory.Register<ushort>(UShortUriKey);
		public static UriKey UShortArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "ushort[]/1.0.0");
		public void UShortArray() => directory.Register<ushort[]>(UShortArrayUriKey);
		#endregion
		#region object
		public static UriKey ObjectUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "object/1.0.0");
		public void Object() => directory.Register<object>(ObjectUriKey);
		public static UriKey ObjectArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "object[]/1.0.0");
		public void ObjectArray() => directory.Register<object[]>(ObjectArrayUriKey);
		#endregion
		#region string
		public static UriKey StringUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "string/1.0.0");
		public void String() => directory.Register<string>(StringUriKey);
		public static UriKey StringArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "string[]/1.0.0");
		public void StringArray() => directory.Register<string[]>(StringArrayUriKey);
		#endregion
		#region dynamic
		public static UriKey DynamicUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "dynamic/1.0.0");
		public void Dynamic() => directory.Register<dynamic>(DynamicUriKey);
		public static UriKey DynamicArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "dynamic[]/1.0.0");
		public void DynamicArray() => directory.Register<dynamic[]>(DynamicArrayUriKey);
		#endregion
		// Additional types
		#region DateTime, DateOnly, TimeOnly
		public static UriKey DateTimeUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "datetime/1.0.0");
		public void DateTime() => directory.Register<DateTime>(DateTimeUriKey);
		public static UriKey DateTimeArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "datetime[]/1.0.0");
		public void DateTimeArray() => directory.Register<DateTime[]>(DateTimeArrayUriKey);
		public static UriKey DateOnlyUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "dateonly/1.0.0");
		public void DateOnly() => directory.Register<DateOnly>(DateOnlyUriKey);
		public static UriKey DateOnlyArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "dateonly[]/1.0.0");
		public void DateOnlyArray() => directory.Register<DateOnly[]>(DateOnlyArrayUriKey);
		public static UriKey TimeOnlyUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "timeonly/1.0.0");
		public void TimeOnly() => directory.Register<TimeOnly>(TimeOnlyUriKey);
		public static UriKey TimeOnlyArrayUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "timeonly[]/1.0.0");
		public void TimeOnlyArray() => directory.Register<TimeOnly>(TimeOnlyArrayUriKey);
		#endregion
		#region JsonNode
		public static UriKey JsonNodeUriKey { get; } = new(UriKey.FuxionSystemTypesBaseUri + "text/json/nodes/jsonnode/1.0.0");
		public void JsonNode() => directory.Register<JsonNode>(JsonNodeUriKey);
		#endregion
	}
	#endregion
}