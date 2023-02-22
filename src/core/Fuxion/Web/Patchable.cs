using System.Collections;
using System.Dynamic;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.CSharp.RuntimeBinder;

namespace Fuxion.Web;

public enum NonExistingPropertiesMode
{
	NotAllowed = 0,
	OnlySet = 1,
	GetAndSet = 2
}

[JsonConverter(typeof(PatchableJsonConverterFactory))]
public sealed class Patchable<T> : DynamicObject where T : class
{
	[JsonConstructor]
	Patchable() : this(NonExistingPropertiesMode.NotAllowed) { }
	public Patchable(NonExistingPropertiesMode nonExistingPropertiesMode = NonExistingPropertiesMode.NotAllowed) => NonExistingPropertiesMode = nonExistingPropertiesMode;
	internal Dictionary<string, (PropertyInfo Property, object? Value)> dic = new();
	//private List<(PropertyInfo Property, string PropertyName, object Value)> Properties = new List<(PropertyInfo Property, string PropertyName, object Value)>();
	//private IDictionary<PropertyInfo, object> _changedProperties = new Dictionary<PropertyInfo, object>();
	//public IEnumerable<string> ChangedPropertyNames { get { return _changedProperties.Keys.Select(p => p.Name); } }

	//public static NonExistingPropertiesMode NonExistingPropertiesMode { get; set; }
	public NonExistingPropertiesMode NonExistingPropertiesMode { get; set; }

	#region SET
	public override bool TrySetMember(SetMemberBinder binder, object? value)
	{
		var pro = typeof(T).GetRuntimeProperty(binder.Name);
		if (pro != null)
		{
			dic[pro.Name] = (pro, value);
			//dic.Add(pro.Name, (pro, value));
			//Properties.Add((pro, pro.Name, value));
			//_changedProperties.Add(pro, value);
			return true;
		}
		switch (NonExistingPropertiesMode)
		{
			case NonExistingPropertiesMode.OnlySet:
			case NonExistingPropertiesMode.GetAndSet:
				// NULLABLE - To review
				dic.Add(binder.Name, (default!, value));
				return true;
			case NonExistingPropertiesMode.NotAllowed:
			default: return base.TrySetMember(binder, value);
		}
	}
	//public void Set(string propName, object val)
	//{
	//    var binder = Binder.SetMember(CSharpBinderFlags.None,
	//           propName, this.GetType(),
	//           new List<CSharpArgumentInfo>{
	//               CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
	//               CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)});
	//    var callsite = CallSite<Func<CallSite, object, object, object>>.Create(binder);

	//    callsite.Target(callsite, this, val);
	//}
	#endregion

	public void Patch(T obj)
	{
		foreach (var pro in dic)
		{
			var property = pro.Value.Property ?? obj.GetType().GetProperty(pro.Key);
			if (property == null) continue;
			var isList = property.PropertyType.GetTypeInfo().IsGenericType && property.PropertyType.IsSubclassOfRawGeneric(typeof(IEnumerable<>));
			if (isList)
			{
				var listType = typeof(List<>).MakeGenericType(property.PropertyType.GenericTypeArguments[0]);
				var list = Activator.CreateInstance(listType) as IList;
				foreach (var item in pro.Value.Value as IList ?? Array.Empty<object>()) list?.Add(item);
				//if (item is JToken jobj)
				//{
				//	var proType = property.PropertyType.GetTypeInfo().GenericTypeArguments[0];
				//	var obj2 = jobj.ToObject(proType);
				//	list?.Add(obj2);
				//}
				property.SetValue(obj, list);
			} else
				property.SetValue(obj, CastValue(property.PropertyType, pro.Value.Value));
		}
	}
	object? CastValue(Type type, object? value)
	{
		var isNullable = type.IsSubclassOfRawGeneric(typeof(Nullable<>));
		var valueType = isNullable ? type.GetTypeInfo().GenericTypeArguments.First() : type;
		object? res = null;
		if (value != null && valueType.GetTypeInfo().IsEnum)
			res = Enum.Parse(valueType, value?.ToString() ?? "");
		else if (value != null && valueType == typeof(Guid))
			res = Guid.Parse(value?.ToString() ?? "");
		else if (value != null) res = Convert.ChangeType(value, valueType);
		if (value != null && isNullable) res = Activator.CreateInstance(typeof(Nullable<>).MakeGenericType(valueType), res);
		return res;
	}
	public Patchable<R> ToPatchable<R>(bool allowNonExistingProperties = false) where R : class
	{
		var res = new Patchable<R>(NonExistingPropertiesMode);
		//var dic = new Dictionary<PropertyInfo, object>();
		//var list = new List<(PropertyInfo Property, string PropertyName, object Value)>();
		var dd = new Dictionary<string, (PropertyInfo Property, object? Value)>();
		foreach (var pair in dic)
			//foreach (var pair in _changedProperties)
		{
			var pro = typeof(R).GetRuntimeProperty(pair.Key);
			if (pro == null && !allowNonExistingProperties) throw new InvalidCastException($"Property '{pair.Key}' cannot be trasfered to type '{typeof(R).Name}'");
			//dic.Add(pro, pair.Value);
			dd.Add(pair.Key, pair.Value);
		}
		//res._changedProperties = dic;
		res.dic = dd;
		return res;
	}
	public bool Has(string memberName) => dic.ContainsKey(memberName);

	//public bool TryGetChangedPropertyValue<TProperty>(Expression<Func<T, TProperty>> exp, out TProperty value)
	//{
	//    if (!_changedProperties.Keys.Any(k => k.Name == exp.GetMemberName()))
	//    {
	//        value = default(TProperty);
	//        return false;
	//    }
	//    var pro = _changedProperties.Single(p => p.Key.Name == exp.GetMemberName());
	//    value = (TProperty)CastValue(typeof(TProperty), pro.Value);
	//    return true;
	//}

	#region GET
	public object? Get(string propertyName)
	{
		var pro = typeof(T).GetRuntimeProperty(propertyName);
		if (dic.ContainsKey(propertyName))
		{
			if (pro == null && (NonExistingPropertiesMode == NonExistingPropertiesMode.NotAllowed || NonExistingPropertiesMode == NonExistingPropertiesMode.OnlySet))
				throw new RuntimeBinderException($"Type '{typeof(T).GetSignature()}' not has a property with name '{propertyName}'");
			return dic[propertyName].Value;
		}
		switch (NonExistingPropertiesMode)
		{
			case NonExistingPropertiesMode.NotAllowed:
			case NonExistingPropertiesMode.OnlySet: throw new RuntimeBinderException($"Type '{GetType().GetSignature()}' not has a property with name '{propertyName}'");
			case NonExistingPropertiesMode.GetAndSet:
			default: return null;
		}
	}
	public override bool TryGetMember(GetMemberBinder binder, out object? result)
	{
		result = Get(binder.Name);
		return true;
		//return base.TryGetMember(binder, out result);
	}
	//public override bool TryGetMember(GetMemberBinder binder, out object result)
	//{
	//    if(Properties.Any(p => p.PropertyName == binder.Name))
	//    {
	//        var pro = Properties.First(p => p.PropertyName == binder.Name);
	//        switch (NonExistingPropertiesMode)
	//        {
	//            case NonExistingPropertiesMode.GetAndSet:
	//                result = pro.Value;
	//                return true;
	//            case NonExistingPropertiesMode.NotAllowed:
	//            case NonExistingPropertiesMode.OnlySet:
	//            default:
	//                return base.TryGetMember(binder, out result);
	//        }
	//    }
	//    return base.TryGetMember(binder, out result);
	//}
	//public object Get(string propName)
	//{

	//    var binder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(CSharpBinderFlags.None,
	//          propName, this.GetType(),
	//          new List<CSharpArgumentInfo>{
	//               CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)});
	//    var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);

	//    return callsite.Target(callsite, this);
	//}
	public TMember Get<TMember>(string memberName)
	{
		var res = Get(memberName);
		if (res != null) return (TMember)CastValue(typeof(TMember), res)!;
		return default!;
	}
	#endregion
}