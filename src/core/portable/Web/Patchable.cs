using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
namespace Fuxion.Web
{
    public sealed class Patchable<T> : DynamicObject where T : class
    {
        private readonly IDictionary<PropertyInfo, object> _changedProperties = new Dictionary<PropertyInfo, object>();
        public IEnumerable<string> ChangedPropertyNames { get { return _changedProperties.Keys.Select(p => p.Name); } }
        public bool TryGetChangedPropertyValue<TProperty>(Expression<Func<T, TProperty>> exp, out TProperty value)
        {
            if (!_changedProperties.Keys.Any(k => k.Name == exp.GetMemberName()))
            {
                value = default(TProperty);
                return false;
            }
            var pro = _changedProperties.Single(p => p.Key.Name == exp.GetMemberName());
            value = (TProperty)CastValue(typeof(TProperty), pro.Value);
            return true;
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var pro = typeof(T).GetRuntimeProperty(binder.Name);
            if (pro != null)
                _changedProperties.Add(pro, value);
            return base.TrySetMember(binder, value);
        }
        public void Patch(T obj)
        {
            foreach (var t in _changedProperties)
            {
                var isList = t.Key.PropertyType.GetTypeInfo().IsGenericType && t.Key.PropertyType.IsSubclassOfRawGeneric(typeof(IEnumerable<>));
                if (isList)
                {
                    var listType = typeof(List<>).MakeGenericType(t.Key.PropertyType.GenericTypeArguments[0]);
                    var list = Activator.CreateInstance(listType) as IList;
                    foreach (var item in t.Value as IList)
                    {
                        if (item is JToken)
                        {
                            var jobj = item as JToken;
                            var proType = t.Key.PropertyType.GetTypeInfo().GenericTypeArguments[0];
                            var obj2 = jobj.ToObject(proType);
                            list.Add(obj2);
                        }
                    }
                    t.Key.SetValue(obj, list);
                }
                else
                {
                    t.Key.SetValue(obj, CastValue(t.Key.PropertyType, t.Value));
                }
            }
        }
        private object CastValue(Type type, object value)
        {
            var isNullable = type.IsSubclassOfRawGeneric(typeof(Nullable<>));
            var valueType = isNullable ? type.GetTypeInfo().GenericTypeArguments.First() : type;
            object res = null;
            if (value != null && valueType.GetTypeInfo().IsEnum)
                res = Enum.Parse(valueType, value.ToString());
            else if (value != null && valueType == typeof(Guid))
                res = Guid.Parse(value.ToString());
            else if (value != null)
                res = Convert.ChangeType(value, valueType);
            if (value != null && isNullable)
                res = Activator.CreateInstance(typeof(Nullable<>).MakeGenericType(valueType), res);
            return res;
        }
    }
}
