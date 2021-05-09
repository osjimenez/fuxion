using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
namespace Fuxion.Synchronization
{
	public class PropertiesComparator<TItemA, TItemB> //: IEnumerable<Tuple<Expression<Func<TItemA, object>>, Expression<Func<TItemB, object>>>>
		where TItemA : class
		where TItemB : class
	{
		private PropertiesComparator() { }
		public static PropertiesComparator<TItemA, TItemB> WithoutAutoDiscoverProperties()
			=> new PropertiesComparator<TItemA, TItemB>();
		public static PropertiesComparator<TItemA, TItemB> WithAutoDiscoverProperties()
		{
			var res = WithoutAutoDiscoverProperties();
			res.AutoPopulateProperties();
			return res;
		}
		private void AutoPopulateProperties()
		{
			var aProps = typeof(TItemA).GetRuntimeProperties();
			var bProps = typeof(TItemB).GetRuntimeProperties();
			var oo = aProps.Select(ap =>
			{
				var bp = bProps.FirstOrDefault(bp2 => bp2.Name == ap.Name && bp2.PropertyType == ap.PropertyType);
				if (bp != null)
					return ((PropertyInfo a, PropertyInfo b)?)(ap, bp);
				return null;
			}).RemoveNulls();
			foreach (var o in oo)
				dic.Add((o.a.Name, o.b.Name), v =>
				{
					var aValue = o.a.GetValue(v.aItem);
					var bValue = o.b.GetValue(v.bItem);
					if (
					(aValue != null && aValue.Equals(bValue))
					||
					(bValue != null && bValue.Equals(aValue))
					||
					(aValue == null && bValue == null)
					|| o.a.DeclaringType == null
					|| o.b.DeclaringType == null
					)
						return null;
					if(Activator.CreateInstance(
						typeof(PropertyRunner<,,,>).MakeGenericType(o.a.DeclaringType, o.b.DeclaringType, o.a.PropertyType, o.b.PropertyType),
						o.a.Name, aValue, bValue, null, null) is IPropertyRunner pr)
						return pr;
					return null;
				});
		}
		internal ICollection<IPropertyRunner> Compare(TItemA itemA, TItemB itemB, bool runInverted)
		{
			var res = new List<IPropertyRunner>();
			foreach (var entry in dic)
			{
				var propertyRunner = entry.Value((itemA, itemB));
				if (propertyRunner != null)
				{
					res.Add(runInverted ? propertyRunner.Invert() : propertyRunner);
				}
			}
			return res;
		}
		Dictionary<(string aPropertyName, string bPropertyName), Func<(TItemA aItem, TItemB bItem), IPropertyRunner?>> dic = new Dictionary<(string aPropertyName, string bPropertyName), Func<(TItemA aItem, TItemB bItem), IPropertyRunner?>>();
		public PropertiesComparator<TItemA, TItemB> With<TPropertyA, TPropertyB>(
			Expression<Func<TItemA, TPropertyA>> aPropertyExpression,
			Expression<Func<TItemB, TPropertyB>> bPropertyExpression)
			=> DoWith(aPropertyExpression, bPropertyExpression);
		public PropertiesComparator<TItemA, TItemB> With<TPropertyA, TPropertyB>(
			Expression<Func<TItemA, TPropertyA>> aPropertyExpression,
			Expression<Func<TItemB, TPropertyB>> bPropertyExpression,
			Func<TPropertyA, string> aNamingFunction,
			Func<TPropertyB, string> bNamingFunction)
			=> DoWith(aPropertyExpression, bPropertyExpression, null, null, aNamingFunction, bNamingFunction);
		public PropertiesComparator<TItemA, TItemB> With<TProperty>(
			Expression<Func<TItemA, TProperty>> aPropertyExpression,
			Expression<Func<TItemB, TProperty>> bPropertyExpression,
			IEqualityComparer<TProperty> comparer)
			=> DoWith(aPropertyExpression, bPropertyExpression, comparer);
		public PropertiesComparator<TItemA, TItemB> With<TProperty>(
			Expression<Func<TItemA, TProperty>> aPropertyExpression,
			Expression<Func<TItemB, TProperty>> bPropertyExpression,
			IEqualityComparer<TProperty> comparer,
			Func<TProperty, string> aNamingFunction,
			Func<TProperty, string> bNamingFunction)
			=> DoWith(aPropertyExpression, bPropertyExpression, comparer, null, aNamingFunction, bNamingFunction);
		public PropertiesComparator<TItemA, TItemB> With<TPropertyA, TPropertyB>(
			Expression<Func<TItemA, TPropertyA>> aPropertyExpression,
			Expression<Func<TItemB, TPropertyB>> bPropertyExpression,
			Func<(TItemA aItem, TPropertyA aValue, TItemB bItem, TPropertyB bValue), bool> comparationFunction)
			=> DoWith(aPropertyExpression, bPropertyExpression, comparationFunction: comparationFunction);
		public PropertiesComparator<TItemA, TItemB> With<TPropertyA, TPropertyB>(
			Expression<Func<TItemA, TPropertyA>> aPropertyExpression,
			Expression<Func<TItemB, TPropertyB>> bPropertyExpression,
			Func<(TItemA aItem, TPropertyA aValue, TItemB bItem, TPropertyB bValue), bool> comparationFunction,
			Func<TPropertyA, string> aNamingFunction,
			Func<TPropertyB, string> bNamingFunction)
			=> DoWith(aPropertyExpression, bPropertyExpression, null, comparationFunction, aNamingFunction, bNamingFunction);
		private PropertiesComparator<TItemA, TItemB> DoWith<TPropertyA, TPropertyB>(
			Expression<Func<TItemA, TPropertyA>> aPropertyExpression,
			Expression<Func<TItemB, TPropertyB>> bPropertyExpression,
			IEqualityComparer<TPropertyA>? comparer = null,
			Func<(TItemA aItem, TPropertyA aValue, TItemB bItem, TPropertyB bValue), bool>? comparationFunction = null,
			Func<TPropertyA, string>? aNamingFunction = null,
			Func<TPropertyB, string>? bNamingFunction = null)
		{
			var tup = (aPropertyExpression.GetMemberName(), bPropertyExpression.GetMemberName());
			if (dic.ContainsKey(tup))
				dic.Remove(tup);
			dic.Add(tup, v =>
			{
				var aValue = aPropertyExpression.Compile().Invoke(v.aItem);
				var bValue = bPropertyExpression.Compile().Invoke(v.bItem);
				bool comparationResult;
				if (comparationFunction != null)
				{
					// Using comparation function
					comparationResult = !comparationFunction((v.aItem, aValue, v.bItem, bValue));
				}
				else
				{
					if (comparer != null)
					{
						// Using comparer
						comparationResult = !comparer.Equals(aValue, (TPropertyA?)Convert.ChangeType(bValue, typeof(TPropertyA)));
					}
					else
					{
						// Comparing values by default
						comparationResult =
							!(
							(aValue != null && aValue.Equals(bValue))
							||
							(bValue != null && bValue.Equals(aValue))
							||
							(aValue == null && bValue == null));
					}
				}
				if (comparationResult)
					return new PropertyRunner<TItemA, TItemB, TPropertyA, TPropertyB>(aPropertyExpression.GetMemberName(),
						aValue,
						bValue,
						aNamingFunction ?? (a => aValue?.ToString() ?? ""),
						bNamingFunction ?? (b => bValue?.ToString() ?? ""));
				return null;
			});
			return this;
		}
	}
}
