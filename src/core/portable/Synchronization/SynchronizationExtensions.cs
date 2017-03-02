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
        internal static ISideRunner CreateRunner(this ISide me)
        {
            return (ISideRunner)Activator.CreateInstance(typeof(SideRunner<,,>).MakeGenericType(me.GetType().GetTypeInfo().GenericTypeArguments), me);
        }
        internal static IComparatorRunner CreateRunner(this IComparator me)
        {
            return (IComparatorRunner)Activator.CreateInstance(typeof(ComparatorRunner<,,>).MakeGenericType(me.GetType().GetTypeInfo().GenericTypeArguments), me);
        }

        internal static Type GetItemType(this ISideRunner me)
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
        internal static Type GetSourceType(this ISideRunner me)
        {
            return me.GetType().GetTypeInfo().GenericTypeArguments[0];
        }
        internal static Tuple<Type, Type> GetItemTypes(this IComparatorRunner me)
        {
            var args = me.GetType().GetTypeInfo().GenericTypeArguments;
            return new Tuple<Type, Type>(args[0], args[1]);
        }
        internal static Type GetKeyType(this IComparator me)
        {
            return me.GetType().GetTypeInfo().GenericTypeArguments[2];
        }
    }
}
