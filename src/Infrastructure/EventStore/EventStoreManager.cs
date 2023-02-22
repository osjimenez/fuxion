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
	public EventStoreStorage(EventStoreClient client, TypeKeyDirectory typeKeyDirectory)
	{
		this.client = client;
		this.typeKeyDirectory = typeKeyDirectory;
	}
	readonly EventStoreClient client;
	readonly TypeKeyDirectory typeKeyDirectory;

	#region IEventStorage
	public async Task CommitAsync(Guid aggregateId, IEnumerable<Event> events)
	{
		var res = await client.AppendToStreamAsync(aggregateId.ToString(), StreamState.Any, events.Select(e => {
			var pod = e.ToEventSourcingPod();
			return new EventData(Uuid.NewUuid(), pod.PayloadKey, JsonSerializer.SerializeToUtf8Bytes(pod));
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
			return stream.Select(e => Encoding.Default.GetString(e.Event.Data.ToArray()).FromJson<EventSourcingPod>(true).WithTypeKeyDirectory(typeKeyDirectory)).RemoveNulls().AsQueryable();
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
			return stream.Select(e => Encoding.Default.GetString(e.Event.Data.ToArray()).FromJson<EventSourcingPod>(true).WithTypeKeyDirectory(typeKeyDirectory)).LastOrDefault();
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
				var pod = Encoding.Default.GetString(e.Event.Data.ToArray()).FromJson<JsonPod<Snapshot, string>>(true);
				return (Snapshot?)pod.As(typeKeyDirectory[pod.PayloadKey]);
			}).LastOrDefault();
		}
		return null;
	}
	public async Task SaveSnapshotAsync(Snapshot snapshot)
	{
		var res = await client.AppendToStreamAsync($"{snapshot.GetType().GetTypeKey()}@{snapshot.AggregateId.ToString()}", StreamState.Any, new[] {
			new EventData(Uuid.NewUuid(), snapshot.GetType().GetTypeKey(), JsonSerializer.SerializeToUtf8Bytes(new JsonPod<Snapshot, string>(snapshot, snapshot.GetType().GetTypeKey())))
		});
	}
	#endregion
}