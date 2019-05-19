using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fuxion.Synchronization
{
	internal static class SynchronizationExtensions
	{
		//internal static ISideRunner CreateRunner(this ISide me, IPrinter printer, IComparatorRunner comparator, ICollection<LoadedItem> entries, IEnumerable<IComparatorResultInternal> results, ICollection<ISideRunner> subSides)
		//      {
		//	return (ISideRunner)Activator.CreateInstance(typeof(SideRunner<,>).MakeGenericType(me.GetType().GetTypeInfo().GenericTypeArguments), me, printer, comparator, entries, results, subSides);
		//      }
		internal static ISideRunner CreateRunner(this ISide me, IPrinter printer) => (ISideRunner)Activator.CreateInstance(typeof(SideRunner<,>).MakeGenericType(me.GetType().GetTypeInfo().GenericTypeArguments), me, printer);
		internal static IComparatorRunner CreateRunner(this IComparator me) => (IComparatorRunner)Activator.CreateInstance(typeof(ComparatorRunner<,,>).MakeGenericType(me.GetType().GetTypeInfo().GenericTypeArguments), me);

		internal static Type GetItemType(this ISideRunner me) => me.GetType().GetTypeInfo().GenericTypeArguments[1];
		internal static ISideRunner SearchMasterSubSide(this ISideRunner me, ISideRunner masterSide)
		{
			var masterTypes = masterSide.GetAllItemsType();
			if (me.Comparator?.GetItemTypes().typeA == me.GetItemType())
				return new[] { masterSide }.Union(masterSide.GetAllSubSides()).Single(s => s.GetItemType() == me.Comparator?.GetItemTypes().typeB);
			else return new[] { masterSide }.Union(masterSide.GetAllSubSides()).Single(s => s.GetItemType() == me.Comparator?.GetItemTypes().typeA);
		}
		internal static IEnumerable<Type> GetAllItemsType(this ISideRunner me)
		{
			IEnumerable<Type> GetAllItemsType(ISideRunner side)
			{
				var r = new List<Type>
				{
					side.GetItemType()
				};
				foreach (var s in side.SubSides)
					r.AddRange(GetAllItemsType(s));
				return r;
			}
			var res = new List<Type>();
			res.AddRange(GetAllItemsType(me));
			return res;
		}
		internal static IEnumerable<ISideRunner> GetAllSubSides(this ISideRunner me)
		{
			IEnumerable<ISideRunner> GetAllSubSides(ISideRunner side)
			{
				var r = new List<ISideRunner>();
				r.AddRange(side.SubSides);
				foreach (var s in side.SubSides)
					r.AddRange(GetAllSubSides(s));
				return r;
			}
			var res = new List<ISideRunner>();
			res.AddRange(GetAllSubSides(me));
			return res;
		}
		internal static Type GetSourceType(this ISideRunner me) => me.GetType().GetTypeInfo().GenericTypeArguments[0];
		internal static (Type typeA, Type typeB) GetItemTypes(this IComparatorRunner me)
		{
			var args = me.GetType().GetTypeInfo().GenericTypeArguments;
			return (args[0], args[1]);
		}
		internal static Type GetKeyType(this IComparator me) => me.GetType().GetTypeInfo().GenericTypeArguments[2];
	}
}
