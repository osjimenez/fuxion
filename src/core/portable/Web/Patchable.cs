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
    public enum NonExistingPropertiesMode
    {
        NotAllowed = 0,
        OnlySet = 1,
        GetAndSet = 2
    }
    public sealed class Patchable<T> : DynamicObject where T : class
    {
        private List<(PropertyInfo Property, string PropertyName, object Value)> Properties = new List<(PropertyInfo Property, string PropertyName, object Value)>();
        //private IDictionary<PropertyInfo, object> _changedProperties = new Dictionary<PropertyInfo, object>();
        //public IEnumerable<string> ChangedPropertyNames { get { return _changedProperties.Keys.Select(p => p.Name); } }
        public static NonExistingPropertiesMode NonExistingPropertiesMode { get; set; }



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
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if(Properties.Any(p => p.PropertyName == binder.Name))
            //if (_changedProperties.Any(p => p.Key.Name == binder.Name))
            {
                var pro = Properties.First(p => p.PropertyName == binder.Name);
                switch (NonExistingPropertiesMode)
                {
                    case NonExistingPropertiesMode.GetAndSet:
                        result = pro.Value;
                        return true;
                    case NonExistingPropertiesMode.NotAllowed:
                    case NonExistingPropertiesMode.OnlySet:
                    default:
                        return base.TryGetMember(binder, out result);
                }
            }
            return base.TryGetMember(binder, out result);
        }
        public object Get(string propName)
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None,
                  propName, this.GetType(),
                  new List<CSharpArgumentInfo>{
                       CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)});
            var callsite = CallSite<Func<CallSite, object, object>>.Create(binder);

            return callsite.Target(callsite, this);
        }
        public TMember Get<TMember>(string memberName) => (TMember)CastValue(typeof(TMember), Get(memberName));
        #endregion
        #region SET
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var pro = typeof(T).GetRuntimeProperty(binder.Name);
            if (pro != null)
            {
                Properties.Add((pro, pro.Name, value));
                //_changedProperties.Add(pro, value);
                return true;
            }
            else
            {
                switch (NonExistingPropertiesMode)
                {
                    case NonExistingPropertiesMode.OnlySet:
                    case NonExistingPropertiesMode.GetAndSet:
                        Properties.Add((null, binder.Name, value));
                        return true;
                    case NonExistingPropertiesMode.NotAllowed:
                    default:
                        return base.TrySetMember(binder, value);
                }
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
            foreach(var pro in Properties)
            //foreach (var t in _changedProperties)
            {
                var isList = pro.Property.PropertyType.GetTypeInfo().IsGenericType && pro.Property.PropertyType.IsSubclassOfRawGeneric(typeof(IEnumerable<>));
                //var isList = t.Key.PropertyType.GetTypeInfo().IsGenericType && t.Key.PropertyType.IsSubclassOfRawGeneric(typeof(IEnumerable<>));
                if (isList)
                {
                    var listType = typeof(List<>).MakeGenericType(pro.Property.PropertyType.GenericTypeArguments[0]);
                    //var listType = typeof(List<>).MakeGenericType(t.Key.PropertyType.GenericTypeArguments[0]);
                    var list = Activator.CreateInstance(listType) as IList;
                    foreach (var item in pro.Value as IList)
                    //foreach (var item in t.Value as IList)
                    {
                        if (item is JToken)
                        {
                            var jobj = item as JToken;
                            var proType = pro.Property.PropertyType.GetTypeInfo().GenericTypeArguments[0];
                            //var proType = t.Key.PropertyType.GetTypeInfo().GenericTypeArguments[0];
                            var obj2 = jobj.ToObject(proType);
                            list.Add(obj2);
                        }
                    }
                    pro.Property.SetValue(obj, list);
                    //t.Key.SetValue(obj, list);
                }
                else
                {
                    pro.Property.SetValue(obj, CastValue(pro.Property.PropertyType, pro.Value));
                    //t.Key.SetValue(obj, CastValue(t.Key.PropertyType, t.Value));
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
        
        

        public Patchable<R> ToPatchable<R>() where R : class
        {
            var res = new Patchable<R>();
            //var dic = new Dictionary<PropertyInfo, object>();
            var list = new List<(PropertyInfo Property, string PropertyName, object Value)>();
            foreach (var pair in Properties)
            //foreach (var pair in _changedProperties)
            {
                var pro = typeof(R).GetRuntimeProperty(pair.PropertyName);
                if (pro == null) throw new InvalidCastException($"Property '{pair.PropertyName}' cannot be trasfered to type '{typeof(R).Name}'");
                //dic.Add(pro, pair.Value);
                list.Add((pro, pro.Name, pair.Value));
            }
            //res._changedProperties = dic;
            res.Properties = list;
            return res;
        }
        public bool Has(string memberName) => Properties.Any(p => p.PropertyName == memberName);
        //public bool Has(string memberName) => ChangedPropertyNames.Contains(memberName);
        //public PatchableHas<T> Has
        //{
        //    get => new PatchableHas<T>(this);
        //}
    }
    //public class PatchableHas<T> where T : class
    //{
    //    public PatchableHas(Patchable<T> patchable)
    //    {
    //        this.patchable = patchable;
    //    }
    //    Patchable<T> patchable;
    //    public bool this[string memberName]
    //    {
    //        get => patchable.ChangedPropertyNames.Contains(memberName);
    //    }
    //}
}
