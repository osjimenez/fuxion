using System.Linq;
using Fuxion.Factories;
using Fuxion.Identity;
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
            //var im = Factory.Get<IdentityManager>();
            var pre = rol.FilterExpression<TSource>(functions);
            //if (pre == null) return Enumerable.Empty<TSource>();
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
            var pre = rol.FilterExpression<TSource>(functions);
            //if (pre == null) return Enumerable.Empty<TSource>().AsQueryable();
            return source.Where(pre);
        }
    }
}
