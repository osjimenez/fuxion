using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace Fuxion.Synchronization
{
    public class Comparator<TItemA, TItemB, TKey> : IComparator
            where TItemA : class
            where TItemB : class
    {
        public Func<TItemA, TKey> OnSelectKeyA { get; set; }
        public Func<TItemB, TKey> OnSelectKeyB { get; set; }
        //public Action<TItemA, TItemB, IComparatorResult> OnCompare { get; set; }
        public Func<TItemA, TItemB, TItemB> OnMapAToB { get; set; }
        public Func<TItemB, TItemA, TItemA> OnMapBToA { get; set; }
        //public IEnumerable<Tuple<Expression<Func<TItemA, object>>, Expression<Func<TItemB, object>>>> OnCompare2 { get; set; }
        public PropertiesComparator<TItemA, TItemB> PropertiesComparator { get; set; } = new PropertiesComparator<TItemA, TItemB>();
    }
    public class PropertiesComparator<TItemA, TItemB> : IEnumerable<Tuple<Expression<Func<TItemA, object>>, Expression<Func<TItemB, object>>>>
        where TItemA : class
        where TItemB : class
    {
        public PropertiesComparator(bool autoComparePropertiesWithSameNameAndType = true)
        {
            if (autoComparePropertiesWithSameNameAndType)
            {
                var aProps = typeof(TItemA).GetRuntimeProperties();
                var bProps = typeof(TItemB).GetRuntimeProperties();
                var oo = aProps.Select(ap =>
                {
                    var bp = bProps.FirstOrDefault(bp2 => bp2.Name == ap.Name && bp2.PropertyType == ap.PropertyType);
                    if (bp != null)
                        return new
                        {
                            a = ap,
                            b = bp
                        };
                    return null;
                }).Where(o=> o != null);
                foreach (var o in oo)
                    dic.Add(new Tuple<string, string>(o.a.Name,o.b.Name), (a, b) => {
                        var aValue = o.a.GetValue(a);
                        var bValue = o.b.GetValue(b);
                        if (
                        (aValue != null && aValue.Equals(bValue))
                        ||
                        (bValue != null && bValue.Equals(aValue))
                        ||
                        (aValue == null && bValue == null)
                        )
                            return null;
                        return (IPropertyRunner)Activator.CreateInstance(
                            typeof(PropertyRunner<,,,>).MakeGenericType(o.a.DeclaringType, o.b.DeclaringType, o.a.PropertyType, o.b.PropertyType),
                            o.a.Name, aValue, bValue, null, null);
                    });
            }
        }
        public int Juas { get; set; }
        internal ICollection<IPropertyRunner> Compare(TItemA itemA, TItemB itemB, bool runInverted)
        {
            var res = new List<IPropertyRunner>();
            //foreach (var i in list)
            //    res.Add(new PropertyRunner<TItemA, TItemB>(
            //        i.Item1.GetMemberName() ?? i.Item2.GetMemberName(),
            //        (TItemA)i.Item1.Compile().Invoke(itemA),
            //        (TItemB)i.Item2.Compile().Invoke(itemB)));
            foreach (var entry in dic)
            {
                var propertyRunner = entry.Value(itemA, itemB);
                if (propertyRunner != null)
                {
                    res.Add(runInverted ? propertyRunner.Invert() : propertyRunner);
                }
            }
            return res;
        }
        //List<Func<TItemA, TItemB, IPropertyRunner>> list = new List<Func<TItemA, TItemB, IPropertyRunner>>();
        Dictionary<Tuple<string, string>, Func<TItemA, TItemB, IPropertyRunner>> dic = new Dictionary<Tuple<string, string>, Func<TItemA, TItemB, IPropertyRunner>>();
        //List<Tuple<Expression<Func<TItemA, object>>, Expression<Func<TItemB, object>>>> list = new List<Tuple<Expression<Func<TItemA, object>>, Expression<Func<TItemB, object>>>>();
        public void Add<TPropertyA, TPropertyB>(
            Expression<Func<TItemA, TPropertyA>> aPropertyExpression,
            Expression<Func<TItemB, TPropertyB>> bPropertyExpression)
        {
            var tup = new Tuple<string, string>(aPropertyExpression.GetMemberName(), bPropertyExpression.GetMemberName());
            if (dic.ContainsKey(tup))
                dic.Remove(tup);
            dic.Add(tup, (a, b) =>
             {
                 var aValue = aPropertyExpression.Compile().Invoke(a);
                 var bValue = bPropertyExpression.Compile().Invoke(b);
                 if (
                 (aValue != null && aValue.Equals(bValue))
                 ||
                 (bValue != null && bValue.Equals(aValue))
                 ||
                 (aValue == null && bValue == null)
                 )
                     return null;
                 return new PropertyRunner<TItemA, TItemB, TPropertyA, TPropertyB>(aPropertyExpression.GetMemberName(),
                         aValue,
                         bValue,
                         null, null);
             });
        }
        public void Add<TPropertyA, TPropertyB>(
            Expression<Func<TItemA, TPropertyA>> aPropertyExpression,
            Expression<Func<TItemB, TPropertyB>> bPropertyExpression,
            Func<TPropertyA, string> aNamingFunction,
            Func<TPropertyB, string> bNamingFunction)
        {
            var tup = new Tuple<string, string>(aPropertyExpression.GetMemberName(), bPropertyExpression.GetMemberName());
            if (dic.ContainsKey(tup))
                dic.Remove(tup);
            dic.Add(tup, (a, b) =>
            {
                var aValue = aPropertyExpression.Compile().Invoke(a);
                var bValue = bPropertyExpression.Compile().Invoke(b);
                if (
                (aValue != null && aValue.Equals(bValue))
                ||
                (bValue != null && bValue.Equals(aValue))
                ||
                (aValue == null && bValue == null)
                )
                    return null;
                return new PropertyRunner<TItemA, TItemB, TPropertyA, TPropertyB>(aPropertyExpression.GetMemberName(),
                        aValue,
                        bValue,
                        aNamingFunction,
                        bNamingFunction);
            });
        }
        public void Add<TProperty>(
            Expression<Func<TItemA, TProperty>> aPropertyExpression,
            Expression<Func<TItemB, TProperty>> bPropertyExpression,
            IEqualityComparer<TProperty> comparer)
        {
            var tup = new Tuple<string, string>(aPropertyExpression.GetMemberName(), bPropertyExpression.GetMemberName());
            if (dic.ContainsKey(tup))
                dic.Remove(tup);
            dic.Add(tup, (a, b) =>
            {
                var aValue = aPropertyExpression.Compile().Invoke(a);
                var bValue = bPropertyExpression.Compile().Invoke(b);
                if (comparer.Equals(aValue, bValue))
                    return new PropertyRunner<TItemA, TItemB, TProperty, TProperty>(aPropertyExpression.GetMemberName(),
                        aValue,
                        bValue,
                        null, null);
                return null;
            });
        }
        public void Add<TProperty>(
            Expression<Func<TItemA, TProperty>> aPropertyExpression,
            Expression<Func<TItemB, TProperty>> bPropertyExpression,
            IEqualityComparer<TProperty> comparer,
            Func<TProperty, string> aNamingFunction,
            Func<TProperty, string> bNamingFunction)
        {
            var tup = new Tuple<string, string>(aPropertyExpression.GetMemberName(), bPropertyExpression.GetMemberName());
            if (dic.ContainsKey(tup))
                dic.Remove(tup);
            dic.Add(tup, (a, b) =>
            {
                var aValue = aPropertyExpression.Compile().Invoke(a);
                var bValue = bPropertyExpression.Compile().Invoke(b);
                if (comparer.Equals(aValue, bValue))
                    return new PropertyRunner<TItemA, TItemB, TProperty, TProperty>(aPropertyExpression.GetMemberName(),
                        aValue,
                        bValue,
                        aNamingFunction,
                        bNamingFunction);
                return null;
            });
        }
        public void Add<TPropertyA, TPropertyB>(
            Expression<Func<TItemA, TPropertyA>> aPropertyExpression,
            Expression<Func<TItemB, TPropertyB>> bPropertyExpression,
            Func<TItemA, TPropertyA, TItemB, TPropertyB, bool> comparationFunction)
        {
            var tup = new Tuple<string, string>(aPropertyExpression.GetMemberName(), bPropertyExpression.GetMemberName());
            if (dic.ContainsKey(tup))
                dic.Remove(tup);
            dic.Add(tup, (a, b) =>
            {
                var aValue = aPropertyExpression.Compile().Invoke(a);
                var bValue = bPropertyExpression.Compile().Invoke(b);
                if (comparationFunction(a, aValue, b, bValue))
                    return new PropertyRunner<TItemA, TItemB, TPropertyA, TPropertyB>(aPropertyExpression.GetMemberName(),
                        aValue,
                        bValue,
                        null, null);
                return null;
            });
        }
        public void Add<TPropertyA, TPropertyB>(
            Expression<Func<TItemA, TPropertyA>> aPropertyExpression,
            Expression<Func<TItemB, TPropertyB>> bPropertyExpression,
            Func<TItemA, TPropertyA, TItemB, TPropertyB, bool> comparationFunction,
            Func<TPropertyA, string> aNamingFunction,
            Func<TPropertyB, string> bNamingFunction)
        {
            var tup = new Tuple<string, string>(aPropertyExpression.GetMemberName(), bPropertyExpression.GetMemberName());
            if (dic.ContainsKey(tup))
                dic.Remove(tup);
            dic.Add(tup, (a, b) =>
            {
                var aValue = aPropertyExpression.Compile().Invoke(a);
                var bValue = bPropertyExpression.Compile().Invoke(b);
                if (comparationFunction(a, aValue, b, bValue))
                    return new PropertyRunner<TItemA, TItemB, TPropertyA, TPropertyB>(aPropertyExpression.GetMemberName(),
                        aValue,
                        bValue,
                        aNamingFunction, 
                        bNamingFunction);
                return null;
            });
        }
        public IEnumerator<Tuple<Expression<Func<TItemA, object>>, Expression<Func<TItemB, object>>>> GetEnumerator() => null;
        IEnumerator IEnumerable.GetEnumerator() => null;
    }
}