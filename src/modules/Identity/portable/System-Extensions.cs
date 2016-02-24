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
        public static IQueryable<TSource> WhereCan<TSource>(this IQueryable<TSource> source, IFunction function)
        {
            var im = Factory.Create<IdentityManager>();
            var pre = im.Current.FilterPredicate<TSource>(function);
            return source.Where(pre);
        }
    }
}
