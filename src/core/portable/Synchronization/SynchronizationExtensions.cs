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
        internal static Type GetItemType(this ISynchronizationSide me)
        {
            return me.GetType().GetTypeInfo().GenericTypeArguments[1];
        }
        internal static ISynchronizationSideInternal SearchMasterSubSide(this ISynchronizationSideInternal me, ISynchronizationSideInternal masterSide)
        {
            var masterTypes = masterSide.GetAllItemsType();
            if (me.Comparator.GetItemTypes().Item1 == me.GetItemType())
                return new[] { masterSide }.Union(masterSide.GetAllSubSides()).Single(s => s.GetItemType() == me.Comparator.GetItemTypes().Item2);
            else return new[] { masterSide }.Union(masterSide.GetAllSubSides()).Single(s => s.GetItemType() == me.Comparator.GetItemTypes().Item1);
        }
        internal static IEnumerable<Type> GetAllItemsType(this ISynchronizationSideInternal me)
        {
            Func<ISynchronizationSideInternal, IEnumerable<Type>> getAllItemsType = null;
            getAllItemsType = new Func<ISynchronizationSideInternal, IEnumerable<Type>>(side => {
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
        internal static IEnumerable<ISynchronizationSideInternal> GetAllSubSides(this ISynchronizationSideInternal me)
        {
            Func<ISynchronizationSideInternal, IEnumerable<ISynchronizationSideInternal>> getAllSubSides = null;
            getAllSubSides = new Func<ISynchronizationSideInternal, IEnumerable<ISynchronizationSideInternal>>(side => {
                var r = new List<ISynchronizationSideInternal>();
                r.AddRange(side.SubSides);
                foreach (var s in side.SubSides)
                    r.AddRange(getAllSubSides(s));
                return r;
            });
            var res = new List<ISynchronizationSideInternal>();
            res.AddRange(getAllSubSides(me));
            //foreach (var s in me.SubSides)
            //    res.AddRange(getAllSubSides(s));
            return res;
        }
        internal static Type GetSourceType(this ISynchronizationSide me)
        {
            return me.GetType().GetTypeInfo().GenericTypeArguments[0];
        }
        internal static Tuple<Type, Type> GetItemTypes(this ISynchronizationComparator me)
        {
            var args = me.GetType().GetTypeInfo().GenericTypeArguments;
            return new Tuple<Type, Type>(args[0], args[1]);
        }
        internal static Type GetKeyType(this ISynchronizationComparator me)
        {
            return me.GetType().GetTypeInfo().GenericTypeArguments[2];
        }
        internal static ISynchronizationProperty Invert(this ISynchronizationProperty me)
        {
            var meType = me.GetType();
            if (meType.IsSubclassOfRawGeneric(typeof(SynchronizationProperty<,>)))
            {
                var resType = typeof(SynchronizationProperty<,>).MakeGenericType(meType.GetTypeInfo().GenericTypeArguments[1], meType.GetTypeInfo().GenericTypeArguments[0]);
                var res = Activator.CreateInstance(resType, me.PropertyName, me.SideValue, me.MasterValue);
                return (ISynchronizationProperty)res;
            }
            else throw new InvalidCastException($"The property '{me.PropertyName}' cannot be inverted because is of type '{meType.Name}' and is not subclass of generic type '{typeof(SynchronizationProperty<,>).Name}'");
        }
        internal static ISynchronizationComparatorResultInternal Invert(this ISynchronizationComparatorResultInternal me)
        {
            var meType = me.GetType();
            if (meType.IsSubclassOfRawGeneric(typeof(SynchronizationComparatorResult<,,>)))
            {
                var resType = typeof(SynchronizationComparatorResult<,,>).MakeGenericType(meType.GetTypeInfo().GenericTypeArguments[1], meType.GetTypeInfo().GenericTypeArguments[0], meType.GetTypeInfo().GenericTypeArguments[2]);
                var res = Activator.CreateInstance(resType);
                resType.GetRuntimeProperty(nameof(me.MasterItem)).SetValue(res, me.SideItem);
                resType.GetRuntimeProperty(nameof(me.SideItem)).SetValue(res, me.MasterItem);
                resType.GetRuntimeProperty(nameof(me.Key)).SetValue(res, me.Key);
                var properties = resType.GetRuntimeProperty(nameof(me.Properties)).GetValue(res);
                var addMethod = typeof(ICollection<ISynchronizationProperty>).GetRuntimeMethod("Add", new[] { typeof(ISynchronizationProperty) });
                foreach (var pro in me.Properties)
                {
                    addMethod.Invoke(properties, new[] { pro.Invert() });
                }
                return (ISynchronizationComparatorResultInternal)res;
            }
            else throw new InvalidCastException($"The item '{me}' cannot be inverted because is of type '{meType.Name}' and is not subclass of generic type '{typeof(SynchronizationComparatorResult<,,>).Name}'");
        }
    }
}
