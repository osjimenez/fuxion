using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    internal static class SynchronizationExtensions
    {
        internal static Type GetItemType(this ISideDefinition me)
        {
            return me.GetType().GetTypeInfo().GenericTypeArguments[1];
        }
        internal static ISideRunner SearchMasterSubSide(this ISideRunner me, ISideRunner masterSide)
        {
            var masterTypes = masterSide.GetAllItemsType();
            if (me.Comparator.GetItemTypes().Item1 == me.GetItemType())
                return new[] { masterSide }.Union(masterSide.GetAllSubSides()).Single(s => s.GetItemType() == me.Comparator.GetItemTypes().Item2);
            else return new[] { masterSide }.Union(masterSide.GetAllSubSides()).Single(s => s.GetItemType() == me.Comparator.GetItemTypes().Item1);
        }
        internal static IEnumerable<Type> GetAllItemsType(this ISideRunner me)
        {
            Func<ISideRunner, IEnumerable<Type>> getAllItemsType = null;
            getAllItemsType = new Func<ISideRunner, IEnumerable<Type>>(side => {
                var r = new List<Type>();
                r.Add(side.GetItemType());
                foreach (var s in side.SubSides)
                    r.AddRange(getAllItemsType(s));
                return r;
            });
            var res = new List<Type>();
            res.AddRange(getAllItemsType(me));
            //res.Add(me.GetItemType());
            //foreach (var s in me.SubSides)
            //    res.AddRange(getAllItemsType(s));
            return res;
        }
        internal static IEnumerable<ISideRunner> GetAllSubSides(this ISideRunner me)
        {
            Func<ISideRunner, IEnumerable<ISideRunner>> getAllSubSides = null;
            getAllSubSides = new Func<ISideRunner, IEnumerable<ISideRunner>>(side => {
                var r = new List<ISideRunner>();
                r.AddRange(side.SubSides);
                foreach (var s in side.SubSides)
                    r.AddRange(getAllSubSides(s));
                return r;
            });
            var res = new List<ISideRunner>();
            res.AddRange(getAllSubSides(me));
            //foreach (var s in me.SubSides)
            //    res.AddRange(getAllSubSides(s));
            return res;
        }
        internal static Type GetSourceType(this ISideDefinition me)
        {
            return me.GetType().GetTypeInfo().GenericTypeArguments[0];
        }
        internal static Tuple<Type, Type> GetItemTypes(this IComparatorDefinition me)
        {
            var args = me.GetType().GetTypeInfo().GenericTypeArguments;
            return new Tuple<Type, Type>(args[0], args[1]);
        }
        internal static Type GetKeyType(this IComparatorDefinition me)
        {
            return me.GetType().GetTypeInfo().GenericTypeArguments[2];
        }
        internal static IProperty Invert(this IProperty me)
        {
            var meType = me.GetType();
            if (meType.IsSubclassOfRawGeneric(typeof(Property<,>)))
            {
                var resType = typeof(Property<,>).MakeGenericType(meType.GetTypeInfo().GenericTypeArguments[1], meType.GetTypeInfo().GenericTypeArguments[0]);
                var res = Activator.CreateInstance(resType, me.PropertyName, me.SideValue, me.MasterValue);
                return (IProperty)res;
            }
            else throw new InvalidCastException($"The property '{me.PropertyName}' cannot be inverted because is of type '{meType.Name}' and is not subclass of generic type '{typeof(Property<,>).Name}'");
        }
        internal static IComparatorResultInternal Invert(this IComparatorResultInternal me)
        {
            var meType = me.GetType();
            if (meType.IsSubclassOfRawGeneric(typeof(ComparatorResult<,,>)))
            {
                var resType = typeof(ComparatorResult<,,>).MakeGenericType(meType.GetTypeInfo().GenericTypeArguments[1], meType.GetTypeInfo().GenericTypeArguments[0], meType.GetTypeInfo().GenericTypeArguments[2]);
                var res = Activator.CreateInstance(resType);
                resType.GetRuntimeProperty(nameof(me.MasterItem)).SetValue(res, me.SideItem);
                resType.GetRuntimeProperty(nameof(me.SideItem)).SetValue(res, me.MasterItem);
                resType.GetRuntimeProperty(nameof(me.Key)).SetValue(res, me.Key);
                var properties = resType.GetRuntimeProperty(nameof(me.Properties)).GetValue(res);
                var addMethod = typeof(ICollection<IProperty>).GetRuntimeMethod("Add", new[] { typeof(IProperty) });
                foreach (var pro in me.Properties)
                {
                    addMethod.Invoke(properties, new[] { pro.Invert() });
                }
                return (IComparatorResultInternal)res;
            }
            else throw new InvalidCastException($"The item '{me}' cannot be inverted because is of type '{meType.Name}' and is not subclass of generic type '{typeof(ComparatorResult<,,>).Name}'");
        }
    }
}
