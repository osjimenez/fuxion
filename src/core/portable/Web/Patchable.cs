using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fuxion.Web
{
    public sealed class Patchable<T> : DynamicObject where T : class
    {
        private IDictionary<PropertyInfo, object> _changedProperties = new Dictionary<PropertyInfo, object>();
        public IEnumerable<string> ChangedPropertyNames { get { return _changedProperties.Keys.Select(p => p.Name); } }
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
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_changedProperties.Any(p => p.Key.Name == binder.Name))
            {
                var pro = _changedProperties.First(p => p.Key.Name == binder.Name).Value;
                if (pro != null)
                {
                    result = pro;
                    return true;
                }
            }
            return base.TryGetMember(binder, out result);
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var pro = typeof(T).GetRuntimeProperty(binder.Name);
            if (pro != null)
            {
                _changedProperties.Add(pro, value);
                return true;
            }
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

        public object GetMember(string propName)
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None,
                  propName, this.GetType(),
                  new List<CSharpArgumentInfo>{
                       CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)});
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);

            return callsite.Target(callsite, this);
        }
        public void SetMember(string propName, object val)
        {
            var binder = Binder.SetMember(CSharpBinderFlags.None,
                   propName, this.GetType(),
                   new List<CSharpArgumentInfo>{
                       CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                       CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)});
            var callsite = CallSite<Func<CallSite, object, object, object>>.Create(binder);

            callsite.Target(callsite, this, val);
        }
        public Patchable<R> ToPatchable<R>() where R : class
        {
            var res = new Patchable<R>();
            var dic = new Dictionary<PropertyInfo, object>();
            foreach (var pair in _changedProperties)
            {
                var pro = typeof(R).GetRuntimeProperty(pair.Key.Name);
                if (pro == null) throw new InvalidCastException($"Property '{pair.Key.Name}' cannot be trasfered to type '{typeof(R).Name}'");
                dic.Add(pro, pair.Value);
            }
            res._changedProperties = dic;
            return res;
        }
    }
}
