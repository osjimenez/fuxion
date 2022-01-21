namespace Microsoft.Extensions.DependencyInjection;

using Fuxion.Application.Events;
using Fuxion.RabbitMQ;
using Fuxion.Reflection;
using RabbitMQ.Client;
using System;

public static class RabbitDIExtensions
{
	public static IFuxionBuilder RabbitMQ(this IFuxionBuilder me, out Func<IServiceProvider, RabbitMQEventBus> builder, string exchangeName, string queueName, string connectionHost = "localhost", int connectionRetryCount = 5, int queueRetryCount = 5)
	{
		me.Services.AddSingleton<IRabbitMQPersistentConnection>(new DefaultRabbitMQPersistentConnection(new ConnectionFactory()
		{
			HostName = connectionHost
		}, connectionRetryCount));
		builder = new Func<IServiceProvider, RabbitMQEventBus>(sp =>
		{
			var bus = new RabbitMQEventBus(
				sp,
				sp.GetRequiredService<IRabbitMQPersistentConnection>(),
				sp.GetRequiredService<TypeKeyDirectory>(),
				exchangeName,
				queueName,
				queueRetryCount);
				// Esto se ejecutará al activar la instancia de RabbitMQEventBus
				// Suscribo los eventos externos. Se manejarán mediante EventHandlers igual que los eventos del propio bounded context
				foreach (var sub in sp.GetServices<EventSubscription>())
				bus.Subscribe(sub.EventType);
			return bus;
		});
		me.Services.AddSingleton(builder);
		me.AddToAutoActivateList<RabbitMQEventBus>();
		return me;
	}
}