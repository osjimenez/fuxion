using EventStore.ClientAPI;
using Fuxion.Reflection;
using Fuxion.EventStore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class EventStoreDIExtensions
	{
		public static IFuxionBuilder EventStore(this IFuxionBuilder me, out Func<IServiceProvider, EventStoreManager> builder, string hostName = "localhost", int port = 1113, string username = "admin", string password = "changeit")
		{
			me.Services.AddSingleton(sp => EventStoreConnection.Create($"ConnectTo=tcp://{username}:{password}@{hostName}:{port}; HeartBeatTimeout=500", ConnectionSettings.Create().KeepReconnecting()).Transform(c => c.ConnectAsync().Wait()));

			builder = new Func<IServiceProvider, EventStoreManager>(sp => new EventStoreManager(sp.GetRequiredService<IEventStoreConnection>(), sp.GetRequiredService<TypeKeyDirectory>()));
			me.Services.AddSingleton(builder);
			return me;
		}
	}
}
