namespace System.Collections.Generic;

using Fuxion;
using Fuxion.Identity;
using static Fuxion.Identity.IdentityExtensions;

public static class System_Extensions
{
	public static IEnumerable<TSource> AuthorizedTo<TSource>(this IEnumerable<TSource> source, params IFunction[] functions)
		=> source.AuthorizedTo(null, functions);
	public static IEnumerable<TSource> AuthorizedTo<TSource>(this IEnumerable<TSource> source, IRol? rol, params IFunction[] functions)
	{
		if (rol == null)
			rol = Singleton.Get<IdentityManager>().GetCurrent();
		if (rol == null)
			throw new InvalidOperationException("Current rol cannot be determined");
		var pre = rol.FilterExpression<TSource>(functions);
		return source is IQueryable<TSource>
			? ((IQueryable<TSource>)source).Where(pre)
			: source.Where(pre.Compile());
	}
	public static IQueryable<TSource> AuthorizedTo<TSource>(this IQueryable<TSource> source, params IFunction[] functions)
		=> source.AuthorizedTo(null, functions);
	public static IQueryable<TSource> AuthorizedTo<TSource>(this IQueryable<TSource> source, IRol? rol, params IFunction[] functions)
	{
		if (rol == null)
			rol = Singleton.Get<IdentityManager>().GetCurrent();
		if (rol == null)
			throw new InvalidOperationException("Current rol cannot be determined");
		var pre = rol.FilterExpression<TSource>(functions);
		return source.Where(pre);
	}
}