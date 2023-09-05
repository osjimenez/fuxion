#if false
using Fuxion.Application.Events;
using Fuxion.RabbitMQ;
using Fuxion.Reflection;
using RabbitMQ.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class RabbitDIExtensions
{
	public static IFuxionBuilder RabbitMQ(this IFuxionBuilder me,
		out Func<IServiceProvider, RabbitMQEventBus> builder,
		string exchangeName,
		string queueName,
		string connectionHost = "localhost",
		int connectionRetryCount = 5,
		int queueRetryCount = 5)
	{
		me.Services.AddSingleton<IRabbitMQPersistentConnection>(new DefaultRabbitMQPersistentConnection(new ConnectionFactory {
			HostName = connectionHost
		}, connectionRetryCount));
		builder = sp => {
			var bus = new RabbitMQEventBus(sp, sp.GetRequiredService<IRabbitMQPersistentConnection>(), sp.GetRequiredService<ITypeKeyResolver>(), exchangeName, queueName, queueRetryCount);
			// Esto se ejecutará al activar la instancia de RabbitMQEventBus
			// Suscribo los eventos externos. Se manejarán mediante EventHandlers igual que los eventos del propio bounded context
			foreach (var sub in sp.GetServices<EventSubscription>()) bus.Subscribe(sub.EventType);
			return bus;
		};
		me.Services.AddSingleton(builder);
		me.AddToAutoActivateList<RabbitMQEventBus>();
		return me;
	}
}
#endif