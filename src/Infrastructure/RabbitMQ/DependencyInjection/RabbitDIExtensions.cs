using Fuxion.Reflection;
using Fuxion.Application.Events;
using Fuxion.Domain;
using Fuxion.RabbitMQ;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class RabbitDIExtensions
	{
		public static ISingularityBuilder RabbitMQ(this ISingularityBuilder me, out Func<IServiceProvider, RabbitMQEventBus> builder, string queueName, string connectionHost = "localhost", int connectionRetryCount = 5, int queueRetryCount = 5)
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
}
