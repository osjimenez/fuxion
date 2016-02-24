using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Fuxion.Factories;

namespace Fuxion.Identity
{
    public static class Extensions
    {
        public static FuxionIdentity FuxionPrincipal(this System.Security.Principal.IPrincipal me)
        {
            return me.Identity as FuxionIdentity;
        }
        //public static IEnumerable<TSource> WhereCan<TSource>(this IEnumerable<TSource> source, IFunction function)
        //{
        //    var im = Factory.Get<IdentityManager>();
        //    var pre = im.Current.FilterPredicate<TSource>(function);
        //    return source is IQueryable<TSource>
        //        ? ((IQueryable<TSource>)source).Where(pre)
        //        : source.Where(pre.Compile());
        //}
        public static IEnumerable<TSource> AuthorizeTo<TSource>(this IEnumerable<TSource> source, params IFunction[] functions)
        {
            var im = Factory.Get<IdentityManager>();
            var pre = im.Current.FilterPredicate<TSource>(functions);
            return source is IQueryable<TSource>
                ? ((IQueryable<TSource>)source).Where(pre)
                : source.Where(pre.Compile());
        }
        //public static IQueryable<TSource> AuthorizeTo<TSource>(this IQueryable<TSource> source, params IFunction[] functions)
        //{
        //    var im = Factory.Get<IdentityManager>();
        //    var pre = im.Current.FilterPredicate<TSource>(functions);
        //    return source.Where(pre);
        //}
    }
}
