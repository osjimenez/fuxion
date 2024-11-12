using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Fuxion.Linq.Expressions;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace Fuxion.Web;

public enum NonExistingPropertiesMode
{
	NotAllowed = 0,
	OnlySet = 1,
	GetAndSet = 2
}

[JsonConverter(typeof(PatchableJsonConverterFactory))]
public sealed class Patchable<T>(NonExistingPropertiesMode nonExistingPropertiesMode = NonExistingPropertiesMode.NotAllowed) : DynamicObject
	where T : notnull
{
	[JsonConstructor]
	Patchable() : this(NonExistingPropertiesMode.NotAllowed) { }
	internal readonly Dictionary<string, (PropertyInfo? Property, object? Value)> Properties = new();
	public NonExistingPropertiesMode NonExistingPropertiesMode { get; set; } = nonExistingPropertiesMode;
	public void Patch(T obj)
	{
		foreach (var pro in Properties)
		{
			var property = pro.Value.Property ?? obj.GetType()
				.GetProperty(pro.Key);
			if (property == null) continue;
			var isList = property.PropertyType.GetTypeInfo()
				.IsGenericType && property.PropertyType.IsSubclassOfRawGeneric(typeof(IEnumerable<>));
			if (isList)
			{
				var listType = typeof(List<>).MakeGenericType(property.PropertyType.GenericTypeArguments[0]);
				var list = Activator.CreateInstance(listType) as IList;
				foreach (var item in pro.Value.Value as IList ?? Array.Empty<object>()) list?.Add(item);
				property.SetValue(obj, list);
			} else
				property.SetValue(obj, CastValue(property.PropertyType, pro.Value.Value));
		}
	}
	object? CastValue(Type type, object? value)
	{
		var isNullable = type.IsSubclassOfRawGeneric(typeof(Nullable<>));
		var valueType = isNullable
			? type.GetTypeInfo()
				.GenericTypeArguments.First()
			: type;
		object? res = null;
		if (value != null && valueType.GetTypeInfo()
			.IsEnum)
			res = Enum.Parse(valueType, value?.ToString() ?? "");
		else if (value != null && valueType == typeof(Guid))
			res = Guid.Parse(value?.ToString() ?? "");
		else if (value != null) res = Convert.ChangeType(value, valueType);
		if (value != null && isNullable) res = Activator.CreateInstance(typeof(Nullable<>).MakeGenericType(valueType), res);
		return res;
	}
	public Patchable<R> ToPatchable<R>(bool allowNonExistingProperties = false)
		where R : class
	{
		var res = new Patchable<R>(NonExistingPropertiesMode);
		foreach (var pair in Properties)
		{
			var pro = typeof(R).GetRuntimeProperty(pair.Key);
			if (pro == null && !allowNonExistingProperties) throw new InvalidCastException($"Property '{pair.Key}' cannot be transferred to type '{typeof(R).Name}'");
			res.Properties.Add(pair.Key, (pro, pair.Value.Value));
		}
		return res;
	}
	public bool Has(string memberName) => Properties.ContainsKey(memberName);
	public static Patchable<T> FromDynamic(Action<dynamic> action, NonExistingPropertiesMode nonExistingPropertiesMode = NonExistingPropertiesMode.NotAllowed)
	{
		dynamic res = new Patchable<T>(nonExistingPropertiesMode);
		action(res);
		return res;
	}
	public static Patchable<T> FromObject(Func<object> func, NonExistingPropertiesMode nonExistingPropertiesMode = NonExistingPropertiesMode.NotAllowed)
	{
		var res = new Patchable<T>(nonExistingPropertiesMode);
		var obj = func();
		foreach (var pro in obj.GetType()
			.GetProperties())
		{
			if (typeof(T).GetProperty(pro.Name) is null && nonExistingPropertiesMode == NonExistingPropertiesMode.NotAllowed)
				throw new RuntimeBinderException($"Type '{typeof(T).GetSignature()}' not has a property with name '{pro.Name}'");
			res.Properties.Add(pro.Name, (pro, pro.GetValue(obj)));
		}
		return res;
	}

	#region SET
	public override bool TrySetMember(SetMemberBinder binder, object? value)
	{
		var pro = typeof(T).GetRuntimeProperty(binder.Name);
		if (pro != null)
		{
			Properties[pro.Name] = (pro, value);
			return true;
		}
		switch (NonExistingPropertiesMode)
		{
			case NonExistingPropertiesMode.OnlySet:
			case NonExistingPropertiesMode.GetAndSet:
				// NULLABLE - To review
				Properties.Add(binder.Name, (null, value));
				return true;
			case NonExistingPropertiesMode.NotAllowed:
			default: return base.TrySetMember(binder, value);
		}
	}
	public void Set(string propName, object val)
	{
		var binder = Binder.SetMember(CSharpBinderFlags.None, propName, GetType(), new List<CSharpArgumentInfo>
		{
			CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
			CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
		});
		var callsite = CallSite<Func<CallSite, object, object, object>>.Create(binder);
		callsite.Target(callsite, this, val);
	}
	#endregion

	#region GET
	public object? Get(string propertyName)
	{
		if (!Properties.TryGetValue(propertyName, out var value))
			return NonExistingPropertiesMode switch
			{
				NonExistingPropertiesMode.NotAllowed or NonExistingPropertiesMode.OnlySet => throw new RuntimeBinderException($"Type '{GetType().GetSignature()}' not has a property with name '{propertyName}'"),
				_ => null
			};
		if (typeof(T).GetRuntimeProperty(propertyName) == null && NonExistingPropertiesMode is NonExistingPropertiesMode.NotAllowed or NonExistingPropertiesMode.OnlySet)
			throw new RuntimeBinderException($"Type '{typeof(T).GetSignature()}' not has a property with name '{propertyName}'");
		return value.Value;
	}
	public override bool TryGetMember(GetMemberBinder binder, out object? result)
	{
		result = Get(binder.Name);
		return true;
	}
	public TValue? Get<TValue>(string memberName)
	{
		var res = Get(memberName);
		if (res != null) return (TValue?)CastValue(typeof(TValue), res);
		return default!;
	}
	public bool TryGet<TValue>(Expression<Func<T, TValue>> memberSelector, [MaybeNullWhen(false)] out TValue value)
	{
		if (Has(memberSelector.GetMemberName()))
		{
			var res = Get<TValue>(memberSelector.GetMemberName());
			value = res ?? throw new InvalidProgramException($"Patchable has '{memberSelector.GetMemberName()}' but Get return null");
			return true;
		}
		value = default;
		return false;
	}
	public bool TryGet<TValue>(string memberName, [MaybeNullWhen(false)] out TValue value)
	{
		if (Has(memberName))
		{
			var res = Get<TValue>(memberName);
			value = res ?? throw new InvalidProgramException($"Patchable has '{memberName}' but Get return null");
			return true;
		}
		value = default;
		return false;
	}
	#endregion
}