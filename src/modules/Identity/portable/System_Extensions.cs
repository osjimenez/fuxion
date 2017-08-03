using System.Linq;
using Fuxion.Factories;
using Fuxion.Identity;
using static Fuxion.Identity.IdentityHelper;
namespace System.Collections.Generic
{
    public static class System_Extensions
    {
        public static IEnumerable<TSource> AuthorizedTo<TSource>(this IEnumerable<TSource> source, params IFunction[] functions)
            => source.AuthorizedTo(null, functions);
        public static IEnumerable<TSource> AuthorizedTo<TSource>(this IEnumerable<TSource> source, IRol rol, params IFunction[] functions)
        {
            if (rol == null)
                rol = Factory.Get<IdentityManager>().GetCurrent();
            var pre = FilterExpression<TSource>(rol, functions);
            return source is IQueryable<TSource>
                ? ((IQueryable<TSource>)source).Where(pre)
                : source.Where(pre.Compile());
        }
        public static IQueryable<TSource> AuthorizedTo<TSource>(this IQueryable<TSource> source, params IFunction[] functions)
            => source.AuthorizedTo(null, functions);
        public static IQueryable<TSource> AuthorizedTo<TSource>(this IQueryable<TSource> source, IRol rol, params IFunction[] functions)
        {
            if (rol == null)
                rol = Factory.Get<IdentityManager>().GetCurrent();
            var pre = FilterExpression<TSource>(rol, functions);
            return source.Where(pre);
        }
    }
}
