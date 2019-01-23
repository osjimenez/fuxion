using Fuxion.Reflection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class EntityFrameworkDIExtensions
	{
		public static IFuxionBuilder EntityFrameworkSqlServer<TContext>(this IFuxionBuilder me, out Func<IServiceProvider, TContext> builder, string dataSource, string initialCatalog, string userID = null, string password = null)
			where TContext : DbContext
		{
			me.Services.AddDbContext<TContext>(options =>
			{
				var connectionBuilder = new SqlConnectionStringBuilder
				{
					DataSource = dataSource,
					InitialCatalog = initialCatalog
				};
				if (string.IsNullOrWhiteSpace(userID) || string.IsNullOrWhiteSpace(password))
					connectionBuilder.IntegratedSecurity = true;
				else
				{
					connectionBuilder.UserID = userID;
					connectionBuilder.Password = password;
				}
				options.UseSqlServer(
					connectionBuilder.ConnectionString,
					sqlServerOptionsAction: sqlOptions =>
					{
						sqlOptions.MigrationsAssembly(typeof(TContext).GetTypeInfo().Assembly.GetName().Name);
						//Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
						sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
					});
			});
			builder = new Func<IServiceProvider, TContext>(sp => sp.GetRequiredService<TContext>());
			return me;
		}
		public static IFuxionBuilder EntityFrameworkInMemory<TContext>(this IFuxionBuilder me, out Func<IServiceProvider, TContext> builder, string databaseName)
			where TContext : DbContext
		{
			me.Services.AddDbContext<TContext>(options =>
			{
				options.UseInMemoryDatabase(databaseName);
			});
			builder = new Func<IServiceProvider, TContext>(sp => sp.GetRequiredService<TContext>());
			return me;
		}
	}
}
