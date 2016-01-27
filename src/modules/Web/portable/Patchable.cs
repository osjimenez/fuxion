using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Web
{
    public sealed class Patchable<T> : DynamicObject where T : class
    {
        private readonly IDictionary<PropertyInfo, object> _changedProperties = new Dictionary<PropertyInfo, object>();
        public IEnumerable<string> ChangedPropertyNames { get { return _changedProperties.Keys.Select(p => p.Name); } }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var pro = typeof(T).GetRuntimeProperty(binder.Name);
            //var pro = typeof(T).GetProperty(binder.Name);
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
                        var itemType = item.GetType();
                        if (itemType == typeof(JObject))
                        {
                            var jobj = item as JObject;
                            var proType = t.Key.PropertyType.GetTypeInfo().GenericTypeArguments[0];
                            var obj2 = jobj.ToObject(proType);
                            list.Add(obj2);
                        }
                    }
                    t.Key.SetValue(obj, list);
                }
                else
                {
                    t.Key.SetValue(obj, PatchProperty(t.Key.PropertyType, t.Value));
                }
            }
        }
        private object PatchProperty(Type type, object value)
        {
            //var isNullable = IsSubclassOfRawGeneric(typeof(Nullable<>), type);
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
        //static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        //{
        //    Queue<Type> toProcess = new Queue<Type>(new[] { toCheck });
        //    while (toProcess.Count > 0)
        //    {
        //        var actual = toProcess.Dequeue();
        //        var cur = actual.GetTypeInfo().IsGenericType ? actual.GetGenericTypeDefinition() : actual;
        //        if (cur.GetTypeInfo().IsGenericType && generic.GetGenericTypeDefinition() == cur.GetGenericTypeDefinition())
        //        {
        //            return true;
        //        }
        //        foreach (var inter in actual.GetTypeInfo().ImplementedInterfaces)
        //            toProcess.Enqueue(inter);
        //        if (actual.GetTypeInfo().BaseType != null)
        //            toProcess.Enqueue(actual.GetTypeInfo().BaseType);
        //    }
        //    return false;
        //}
    }
}
