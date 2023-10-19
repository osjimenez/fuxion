using Fuxion.Domain;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Fuxion.AspNetCore;

public class NexusHub(SignalRServerSubscriber subscriber, IServiceProvider serviceProvider) : Hub<INexuxHubClient>
{
	[HubMethodName("SendToServer")]
	public void ReceiveFromClient(object message)
	{
		using var scope = serviceProvider.CreateScope();
		subscriber.ReceiveFromClient(new Receipt<object>(message, scope.ServiceProvider));
	}
}

public interface INexuxHubClient
{
	public Task SendToClient(object message);
}

public class SignalRServerPublisher(IHubContext<NexusHub,INexuxHubClient> hub) : IPublisher<object>
{
	public PublisherInfo Info { get; } = new("");
	public Task Publish(object message) => hub.Clients.All.SendToClient(message);
}

public class SignalRServerSubscriber(IHubContext<NexusHub, INexuxHubClient> hub) : ISubscriber<object>
{
	List<Action<IReceipt<object>>> receivers = new();
	public Task Initialize() => Task.CompletedTask;
	public void Attach(INexus nexus) { }
	public Task<IDisposable> OnReceive(Action<IReceipt<object>> onMessageReceived)
	{
		receivers.Add(onMessageReceived);
		var dis = onMessageReceived.AsDisposable(f => receivers.Remove(f));
		return Task.FromResult<IDisposable>(dis);
	}
	internal void ReceiveFromClient(IReceipt<object> receipt)
	{
		foreach (var receive in receivers)
		{
			receive(receipt);
		}
	}
}
#if NET8_0
public class SignalRClientPublisher([FromKeyedServices("NexusHub")] HubConnection nexusHub) : IPublisher<object>
{
	public PublisherInfo Info { get; } = new("");
	public async Task Publish(object message)
	{
		await nexusHub.InvokeAsync("SendToServer", message);
	}
}
public class SignalRClientSubscriber([FromKeyedServices("NexusHub")] HubConnection nexusHub, IServiceProvider serviceProvider) : ISubscriber<object>
{
	List<Action<IReceipt<object>>> receivers = new();
	public Task Initialize() => Task.CompletedTask;
	public void Attach(INexus nexus)
	{
		nexusHub.On<object>("SendToClient", message =>
		{
			using var scope = serviceProvider.CreateScope();
			foreach (var receive in receivers)
			{
				receive(new Receipt<object>(message, scope.ServiceProvider));
			}
		});
	}
	public Task<IDisposable> OnReceive(Action<IReceipt<object>> onMessageReceived)
	{
		receivers.Add(onMessageReceived);
		var dis = onMessageReceived.AsDisposable(f => receivers.Remove(f));
		return Task.FromResult<IDisposable>(dis);
	}
}
#endif