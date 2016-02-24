using System.Linq;
using Fuxion.Factories;
using Fuxion.Identity;
namespace System.Collections.Generic
{
    public static class Extensions
    {
        public static IEnumerable<TSource> AuthorizedTo<TSource>(this IEnumerable<TSource> source, params IFunction[] functions)
        {
            var im = Factory.Get<IdentityManager>();
            var pre = im.Current.FilterExpression<TSource>(functions);
            return source is IQueryable<TSource>
                ? ((IQueryable<TSource>)source).Where(pre)
                : source.Where(pre.Compile());
        }
    }
}
