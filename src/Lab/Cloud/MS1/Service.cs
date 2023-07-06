using System.Reactive.Linq;
using Fuxion.Domain;
using Fuxion.Lab.Common;

namespace Fuxion.Lab.Cloud.MS1;

public class Service(IServiceProvider serviceProvider) : IHostedService
{
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using var scope = serviceProvider.CreateScope();
		var nexus = scope.ServiceProvider.GetRequiredService<INexus>();
		var resolver = scope.ServiceProvider.GetRequiredService<IUriKeyResolver>();
		await nexus.Initialize();
		var rabbitConnection = scope.ServiceProvider.GetRequiredService<RabbitMQConnection>();
		var rabbitSubscriber = scope.ServiceProvider.GetRequiredService<RabbitMQSubscriber>();
		rabbitSubscriber.Attach(nexus);
		await rabbitConnection.Initialize();
		await rabbitSubscriber.Initialize();
		await nexus.OnReceive(msg =>
		{
			Console.WriteLine("Message received:\r\n" + msg);
		});
		nexus.Observe(obj => true)
			.OfType<RabbitMQReceive>()
			.Select(receive => receive.Body
				.ToArray()
				.BuildUriKeyPod(resolver)
				.FromUtf8Bytes()
				.FromJsonNode().Pod
			)
			.Buffer(2)
			.Subscribe(list =>
			{
				//Console.WriteLine("Message observed from nexus extensions:\r\n" + Encoding.UTF8.GetString(list.Body.ToArray()));
				foreach(var msg in list)
					if (msg.Payload is TestMessage tm)
						Console.WriteLine("Message observed from nexus extensions:\r\n" + tm.Name);
			});
		new ObservableNexusDecorator(nexus).Observe(obj => true)
			.Subscribe(msg =>
			{
				Console.WriteLine("Message observed from nexus:\r\n" + msg);
			});
	}
	public Task StopAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}