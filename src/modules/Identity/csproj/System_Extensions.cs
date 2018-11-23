using Fuxion.Factories;
using Fuxion.Identity;
using System.Linq;
using static Fuxion.Identity.IdentityExtensions;
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
			var pre = rol.FilterExpression<TSource>(functions);
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
			return source.Where(pre);
		}
	}
}
