using EventStore.Client;
using Fuxion.EventStore;
using Fuxion.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class EventStoreDIExtensions
{
	public static IFuxionBuilder EventStore(this IFuxionBuilder me,
		out Func<IServiceProvider, EventStoreStorage> builder,
		string hostName = "localhost",
		int port = 2113,
		string username = "admin",
		string password = "changeit")
	{
		// https://developers.eventstore.com/clients/grpc/#connection-details
		var connectionString = $"esdb+discover://{hostName}:{port}?tls=false&keepAliveTimeout=10000&keepAliveInterval=10000";
		var settings = EventStoreClientSettings.Create(connectionString);
		var client = new EventStoreClient(settings);
		me.Services.AddSingleton(client);
		builder = sp => new(sp.GetRequiredService<EventStoreClient>(), sp.GetRequiredService<ITypeKeyResolver>());
		me.Services.AddSingleton(builder);
		return me;
	}
}