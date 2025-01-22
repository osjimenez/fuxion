using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Nodes;
using Fuxion.Reflection;

namespace Fuxion.Pods;

public interface IUriKeyResolver
{
	Type this[UriKey key] { get; }
	UriKey this[Type type] { get; }
	bool ContainsKey(UriKey key);
	bool ContainsKey(Type type);
	UriKey? GetFullKey(UriKey key);
}

public class UriKeyResolverNotFoundException(string message) : FuxionException(message) { }

public class UriKeyDirectory : IUriKeyResolver
{
	public UriKeyDirectory()
	{
		SystemRegister = new(this);
	}
	readonly Dictionary<UriKey, Type> _keyToTypeDictionary = new();
	readonly Dictionary<Type, UriKey> _typeToKeyDictionary = new(
#if !NETSTANDARD2_0 && !NET462
		ReferenceEqualityComparer.Instance
#endif
		);
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
	public UriKey? GetFullKey(UriKey key) => _keyToTypeDictionary.Keys.FirstOrDefault(k => k.Equals(key));

	public void RegisterAssemblyOf(Type type, Func<(Type Type, UriKeyAttribute? Attribute), bool>? predicate = null) =>
		RegisterAssembly(type.Assembly, predicate);
	public void RegisterAssemblyOf<T>(Func<(Type Type, UriKeyAttribute? Attribute), bool>? predicate = null) =>
		RegisterAssembly(typeof(T).Assembly, predicate);
	public void RegisterAssembly(Assembly assembly, Func<(Type Type, UriKeyAttribute? Attribute), bool>? predicate = null)
	{
		var query = assembly.GetTypes()
			.Where(t => t.HasCustomAttribute<UriKeyAttribute>(false))
			.Select(t => (Type: t, Attribute: t.GetCustomAttribute<UriKeyAttribute>(true, false)));
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
			// TODO Al parecer colisionan con el object, dice que la clave ya ha sido registrada
			// Dynamic();
			// DynamicArray();
			// Additional types
			DateTime();
			DateTimeArray();
#if !NETSTANDARD2_0 && !NET462
			DateOnly();
			DateOnlyArray();
			TimeOnly();
			TimeOnlyArray();
#endif
			JsonNode();
			JsonObject();
		}
		// Built-in types
		// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/built-in-types
		#region bool
		public void Bool() => directory.Register<bool>(SystemUriKeys.Bool);
		public void BoolArray() => directory.Register<bool[]>(SystemUriKeys.BoolArray);
		#endregion
		#region byte
		public void Byte() => directory.Register<byte>(SystemUriKeys.Byte);
		public void ByteArray() => directory.Register<byte[]>(SystemUriKeys.ByteArray);
		public void SByte() => directory.Register<sbyte>(SystemUriKeys.SByte);
		public void SByteArray() => directory.Register<sbyte[]>(SystemUriKeys.SByteArray);
		#endregion
		#region char
		public void Char() => directory.Register<char>(SystemUriKeys.Char);
		public void CharArray() => directory.Register<char[]>(SystemUriKeys.CharArray);
		#endregion
		#region decimal
		public void Decimal() => directory.Register<decimal>(SystemUriKeys.Decimal);
		public void DecimalArray() => directory.Register<decimal[]>(SystemUriKeys.DecimalArray);
		#endregion
		#region double
		public void Double() => directory.Register<double>(SystemUriKeys.Double);
		public void DoubleArray() => directory.Register<double[]>(SystemUriKeys.DoubleArray);
		#endregion
		#region float
		public void Float() => directory.Register<float>(SystemUriKeys.Float);
		public void FloatArray() => directory.Register<float[]>(SystemUriKeys.FloatArray);
		#endregion
		#region int
		public void Int() => directory.Register<int>(SystemUriKeys.Int);
		public void IntArray() => directory.Register<int[]>(SystemUriKeys.IntArray);
		public void UInt() => directory.Register<uint>(SystemUriKeys.UInt);
		public void UIntArray() => directory.Register<uint[]>(SystemUriKeys.UIntArray);
		public void NInt() => directory.Register<nint>(SystemUriKeys.NInt);
		public void NIntArray() => directory.Register<nint[]>(SystemUriKeys.NIntArray);
		public void NUInt() => directory.Register<nuint>(SystemUriKeys.NUInt);
		public void NUIntArray() => directory.Register<nuint[]>(SystemUriKeys.NUIntArray);
		#endregion
		#region long
		public void Long() => directory.Register<long>(SystemUriKeys.Long);
		public void LongArray() => directory.Register<long[]>(SystemUriKeys.LongArray);
		public void ULong() => directory.Register<ulong>(SystemUriKeys.ULong);
		public void ULongArray() => directory.Register<ulong[]>(SystemUriKeys.ULongArray);
		#endregion
		#region short
		public void Short() => directory.Register<short>(SystemUriKeys.Short);
		public void ShortArray() => directory.Register<short[]>(SystemUriKeys.ShortArray);
		public void UShort() => directory.Register<ushort>(SystemUriKeys.UShort);
		public void UShortArray() => directory.Register<ushort[]>(SystemUriKeys.UShortArray);
		#endregion
		#region object
		public void Object() => directory.Register<object>(SystemUriKeys.Object);
		public void ObjectArray() => directory.Register<object[]>(SystemUriKeys.ObjectArray);
		#endregion
		#region string
		public void String() => directory.Register<string>(SystemUriKeys.String);
		public void StringArray() => directory.Register<string[]>(SystemUriKeys.StringArray);
		#endregion
		#region dynamic
		// TODO typeof(dynamic) no se puede
		// public void Dynamic() => directory.Register<dynamic>(SystemUriKeys.Dynamic);
		// public void DynamicArray() => directory.Register<dynamic[]>(SystemUriKeys.DynamicArray);
		#endregion
		// Additional types
		#region DateTime, DateOnly, TimeOnly
		public void DateTime() => directory.Register<DateTime>(SystemUriKeys.DateTime);
		public void DateTimeArray() => directory.Register<DateTime[]>(SystemUriKeys.DateTimeArray);
#if !NETSTANDARD2_0 && !NET462
		public void DateOnly() => directory.Register<DateOnly>(SystemUriKeys.DateOnly);
		public void DateOnlyArray() => directory.Register<DateOnly[]>(SystemUriKeys.DateOnlyArray);
		public void TimeOnly() => directory.Register<TimeOnly>(SystemUriKeys.TimeOnly);
		public void TimeOnlyArray() => directory.Register<TimeOnly[]>(SystemUriKeys.TimeOnlyArray);
#endif
		#endregion
		#region JsonNode
		public void JsonNode() => directory.Register<JsonNode>(SystemUriKeys.JsonNode);
		public void JsonObject() => directory.Register<JsonObject>(SystemUriKeys.JsonObject);
		#endregion
	}
	#endregion
}

public static class SystemUriKeys
{
	static readonly Dictionary<Type, UriKey> Dic = new();
	static SystemUriKeys()
	{
		Dic.Add(typeof(bool), Bool);
		Dic.Add(typeof(bool[]), BoolArray);
		Dic.Add(typeof(byte), Byte);
		Dic.Add(typeof(byte[]), ByteArray);
		Dic.Add(typeof(sbyte), SByte);
		Dic.Add(typeof(sbyte[]), SByteArray);
		Dic.Add(typeof(char), Char);
		Dic.Add(typeof(char[]), CharArray);
		Dic.Add(typeof(decimal), Decimal);
		Dic.Add(typeof(decimal[]), DecimalArray);
		Dic.Add(typeof(double), Double);
		Dic.Add(typeof(double[]), DoubleArray);
		Dic.Add(typeof(float), Float);
		Dic.Add(typeof(float[]), FloatArray);
		Dic.Add(typeof(int), Int);
		Dic.Add(typeof(int[]), IntArray);
		Dic.Add(typeof(uint), UInt);
		Dic.Add(typeof(uint[]), UIntArray);
		Dic.Add(typeof(nint), NInt);
		Dic.Add(typeof(nint[]), NIntArray);
		Dic.Add(typeof(nuint), NUInt);
		Dic.Add(typeof(nuint[]), NUIntArray);
		Dic.Add(typeof(long), Long);
		Dic.Add(typeof(long[]), LongArray);
		Dic.Add(typeof(ulong), ULong);
		Dic.Add(typeof(ulong[]), ULongArray);
		Dic.Add(typeof(short), Short);
		Dic.Add(typeof(short[]), ShortArray);
		Dic.Add(typeof(ushort), UShort);
		Dic.Add(typeof(ushort[]), UShortArray);
		Dic.Add(typeof(object), Object);
		Dic.Add(typeof(object[]), ObjectArray);
		Dic.Add(typeof(string), String);
		Dic.Add(typeof(string[]), StringArray);
		// TODO typeof(dynamic) no se puede
		// Dic.Add(typeof(dynamic), Dynamic);
		// Dic.Add(typeof(dynamic[]), DynamicArray);
		Dic.Add(typeof(DateTime), DateTime);
		Dic.Add(typeof(DateTime[]), DateTimeArray);
#if !NETSTANDARD2_0 && !NET462
		Dic.Add(typeof(DateOnly), DateOnly);
		Dic.Add(typeof(DateOnly[]), DateOnlyArray);
		Dic.Add(typeof(TimeOnly), TimeOnly);
		Dic.Add(typeof(TimeOnly[]), TimeOnlyArray);
#endif
		Dic.Add(typeof(JsonNode), JsonNode);
		Dic.Add(typeof(JsonObject), JsonObject);
	}
	#region bool
	public static UriKey Bool { get; } = new(UriKey.FuxionSystemTypesBaseUri + "bool/1.0.0");
	public static UriKey BoolArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "bool[]/1.0.0");
	#endregion
	#region byte
	public static UriKey Byte { get; } = new(UriKey.FuxionSystemTypesBaseUri + "byte/1.0.0");
	public static UriKey ByteArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "byte[]/1.0.0");
	public static UriKey SByte { get; } = new(UriKey.FuxionSystemTypesBaseUri + "sbyte/1.0.0");
	public static UriKey SByteArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "sbyte[]/1.0.0");
	#endregion
	#region char
	public static UriKey Char { get; } = new(UriKey.FuxionSystemTypesBaseUri + "char/1.0.0");
	public static UriKey CharArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "char[]/1.0.0");
	#endregion
	#region decimal
	public static UriKey Decimal { get; } = new(UriKey.FuxionSystemTypesBaseUri + "decimal/1.0.0");
	public static UriKey DecimalArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "decimal[]/1.0.0");
	#endregion
	#region double
	public static UriKey Double { get; } = new(UriKey.FuxionSystemTypesBaseUri + "double/1.0.0");
	public static UriKey DoubleArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "double[]/1.0.0");
	#endregion
	#region float
	public static UriKey Float { get; } = new(UriKey.FuxionSystemTypesBaseUri + "float/1.0.0");
	public static UriKey FloatArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "float[]/1.0.0");
	#endregion
	#region int
	public static UriKey Int { get; } = new(UriKey.FuxionSystemTypesBaseUri + "int/1.0.0");
	public static UriKey IntArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "int[]/1.0.0");
	public static UriKey UInt { get; } = new(UriKey.FuxionSystemTypesBaseUri + "uint/1.0.0");
	public static UriKey UIntArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "uint[]/1.0.0");
	public static UriKey NInt { get; } = new(UriKey.FuxionSystemTypesBaseUri + "nint/1.0.0");
	public static UriKey NIntArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "nint[]/1.0.0");
	public static UriKey NUInt { get; } = new(UriKey.FuxionSystemTypesBaseUri + "nuint/1.0.0");
	public static UriKey NUIntArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "nuint[]/1.0.0");
	#endregion
	#region long
	public static UriKey Long { get; } = new(UriKey.FuxionSystemTypesBaseUri + "long/1.0.0");
	public static UriKey LongArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "long[]/1.0.0");
	public static UriKey ULong { get; } = new(UriKey.FuxionSystemTypesBaseUri + "ulong/1.0.0");
	public static UriKey ULongArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "ulong[]/1.0.0");
	#endregion
	#region short
	public static UriKey Short { get; } = new(UriKey.FuxionSystemTypesBaseUri + "short/1.0.0");
	public static UriKey ShortArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "short[]/1.0.0");
	public static UriKey UShort { get; } = new(UriKey.FuxionSystemTypesBaseUri + "ushort/1.0.0");
	public static UriKey UShortArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "ushort[]/1.0.0");
	#endregion
	#region object
	public static UriKey Object { get; } = new(UriKey.FuxionSystemTypesBaseUri + "object/1.0.0");
	public static UriKey ObjectArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "object[]/1.0.0");
	#endregion
	#region string
	public static UriKey String { get; } = new(UriKey.FuxionSystemTypesBaseUri + "string/1.0.0");
	public static UriKey StringArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "string[]/1.0.0");
	#endregion
	#region dynamic
	public static UriKey Dynamic { get; } = new(UriKey.FuxionSystemTypesBaseUri + "dynamic/1.0.0");
	public static UriKey DynamicArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "dynamic[]/1.0.0");
	#endregion
	// Additional types
	#region DateTime, DateOnly, TimeOnly
	public static UriKey DateTime { get; } = new(UriKey.FuxionSystemTypesBaseUri + "datetime/1.0.0");
	public static UriKey DateTimeArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "datetime[]/1.0.0");
	public static UriKey DateOnly { get; } = new(UriKey.FuxionSystemTypesBaseUri + "dateonly/1.0.0");
	public static UriKey DateOnlyArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "dateonly[]/1.0.0");
	public static UriKey TimeOnly { get; } = new(UriKey.FuxionSystemTypesBaseUri + "timeonly/1.0.0");
	public static UriKey TimeOnlyArray { get; } = new(UriKey.FuxionSystemTypesBaseUri + "timeonly[]/1.0.0");
	#endregion
	#region JsonNode
	public static UriKey JsonNode { get; } = new(UriKey.FuxionSystemTypesBaseUri + "text/json/nodes/jsonnode/1.0.0");
	// TODO JsonObject extiende de JsonNode, pero la UriKey no es una cadena. Probar a definirla como cadena
	public static UriKey JsonObject { get; } = new(UriKey.FuxionSystemTypesBaseUri + "text/json/nodes/jsonobject/1.0.0");
	#endregion
	public static bool TryGetFor(Type type, [MaybeNullWhen(returnValue: false)] out UriKey uriKey) => Dic.TryGetValue(type, out uriKey);
	public static bool TryGetFor<T>([MaybeNullWhen(returnValue: false)] out UriKey uriKey) => TryGetFor(typeof(T), out uriKey);
	public static UriKey GetFor(Type type)
	{
		if (Dic.TryGetValue(type, out var uriKey)) return uriKey;
		throw new InvalidOperationException("Only system types are allowed as generic types");
	}
	public static UriKey GetFor<T>() => GetFor(typeof(T));
}