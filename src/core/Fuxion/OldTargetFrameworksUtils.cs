namespace System
{
	public static class SystemExtensions
	{
#if NETSTANDARD2_0 || NET462
		public static bool EndsWith(this string me, char c) => me.EndsWith(c.ToString());
		public static bool StartsWith(this string me, char c) => me.StartsWith(c.ToString());

		public static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source, int count) => source.Take(source.Count() - count);
		public static TSource? MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
			=> source.OrderByDescending(keySelector)
				.FirstOrDefault();
#endif
	}
}