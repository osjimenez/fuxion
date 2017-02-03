using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Synchronization
{
    public static class SynchronizationExtensions
    {
        internal static Type GetItemType(this ISynchronizationSide me)
        {
            return me.GetType().GetTypeInfo().GenericTypeArguments[1];
        }
        internal static Tuple<Type, Type> GetItemTypes(this ISynchronizationComparator me)
        {
            return new Tuple<Type, Type>(me.GetType().GetTypeInfo().GenericTypeArguments[0], me.GetType().GetTypeInfo().GenericTypeArguments[1]);
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
