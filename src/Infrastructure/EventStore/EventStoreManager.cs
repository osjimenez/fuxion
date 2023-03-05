using System.Text;
using System.Text.Json;
using EventStore.Client;
using Fuxion.Application.Events;
using Fuxion.Application.Snapshots;
using Fuxion.Domain;
using Fuxion.Json;
using Fuxion.Reflection;

namespace Fuxion.EventStore;

public class EventStoreStorage : IEventStorage, ISnapshotStorage
{
	public EventStoreStorage(EventStoreClient client, ITypeKeyResolver typeKeyResolver)
	{
		this.client = client;
		this.typeKeyResolver = typeKeyResolver;
	}
	readonly EventStoreClient client;
	readonly ITypeKeyResolver typeKeyResolver;

	#region IEventStorage
	public async Task CommitAsync(Guid aggregateId, IEnumerable<Event> events)
	{
		var res = await client.AppendToStreamAsync(aggregateId.ToString(), StreamState.Any, events.Select(e => {
			var pod = e.ToEventSourcingPod();
			return new EventData(
				Uuid.NewUuid(),
				pod.Discriminator.ToString(),
				JsonSerializer.SerializeToUtf8Bytes(pod));
		}).ToArray());
	}
	public async Task<IQueryable<Event>> GetEventsAsync(Guid aggregateId, int start, int count)
	{
		//TODO - Implementar un mecanismo de paginación?
		var res = client.ReadStreamAsync(Direction.Forwards, aggregateId.ToString(), (ulong)start, count);
		var state = await res.ReadState;
		if (state == ReadState.Ok)
		{
			var stream = await res.ToListAsync();
			return stream.Select(e => Encoding.Default.GetString(e.Event.Data.ToArray()).FromJson<EventSourcingPod>(true).WithTypeKeyResolver(typeKeyResolver)).RemoveNulls().AsQueryable();
		}
		return Array.Empty<Event>().AsQueryable();
	}
	public async Task<Event?> GetLastEventAsync(Guid aggregateId)
	{
		var res = client.ReadStreamAsync(Direction.Backwards, aggregateId.ToString(), StreamPosition.End, 1);
		var state = await res.ReadState;
		if (state == ReadState.Ok)
		{
			var stream = await res.ToListAsync();
			return stream.Select(e => Encoding.Default.GetString(e.Event.Data.ToArray()).FromJson<EventSourcingPod>(true).WithTypeKeyResolver(typeKeyResolver)).LastOrDefault();
		}
		return null;
	}
	#endregion

	#region ISnapshotStorage
	public async Task<Snapshot?> GetSnapshotAsync(Type snapshotType, Guid aggregateId)
	{
		var res = client.ReadStreamAsync(Direction.Backwards, $"{snapshotType.GetTypeKey()}@{aggregateId.ToString()}", StreamPosition.End, 1);
		var state = await res.ReadState;
		if (state == ReadState.Ok)
		{
			var stream = await res.ToListAsync();
			return stream.Select(e => {
				var pod = Encoding.Default.GetString(e.Event.Data.ToArray()).FromJson<JsonPod<TypeKey, Snapshot>>(true);
				return (Snapshot?)pod.As(typeKeyResolver[pod.Discriminator]);
			}).LastOrDefault();
		}
		return null;
	}
	public async Task SaveSnapshotAsync(Snapshot snapshot)
	{
		var res = await client.AppendToStreamAsync($"{snapshot.GetType().GetTypeKey()}@{snapshot.AggregateId.ToString()}", StreamState.Any, new[] {
			new EventData(
				Uuid.NewUuid(),
				snapshot.GetType().GetTypeKey().ToString(),
				JsonSerializer.SerializeToUtf8Bytes(new JsonPod<TypeKey, Snapshot>(snapshot.GetType().GetTypeKey(), snapshot)))
		});
	}
	#endregion
}