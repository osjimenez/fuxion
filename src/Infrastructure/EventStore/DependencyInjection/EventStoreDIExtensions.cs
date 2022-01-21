namespace Microsoft.Extensions.DependencyInjection;

using EventStore.ClientAPI;
using Fuxion.EventStore;
using Fuxion.Reflection;

public static class EventStoreDIExtensions
{
	public static IFuxionBuilder EventStore(this IFuxionBuilder me, out Func<IServiceProvider, EventStoreStorage> builder, string hostName = "localhost", int port = 1113, string username = "admin", string password = "changeit")
	{
		me.Services.AddSingleton(sp => EventStoreConnection.Create($"ConnectTo=tcp://{username}:{password}@{hostName}:{port}; HeartBeatTimeout=500", ConnectionSettings.Create().KeepReconnecting()).Transform(c => c.ConnectAsync().Wait()));

		builder = new Func<IServiceProvider, EventStoreStorage>(sp => new EventStoreStorage(sp.GetRequiredService<IEventStoreConnection>(), sp.GetRequiredService<TypeKeyDirectory>()));
		me.Services.AddSingleton(builder);
		return me;
	}
}