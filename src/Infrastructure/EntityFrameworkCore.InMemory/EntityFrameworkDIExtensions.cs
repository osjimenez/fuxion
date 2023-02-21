using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class EntityFrameworkDIExtensions
{
	public static IFuxionBuilder EntityFrameworkInMemory<TContext>(this IFuxionBuilder me, out Func<IServiceProvider, TContext> builder, string databaseName) where TContext : DbContext
	{
		me.Services.AddDbContext<TContext>(options => { options.UseInMemoryDatabase(databaseName); });
		builder = sp => sp.GetRequiredService<TContext>();
		return me;
	}
}