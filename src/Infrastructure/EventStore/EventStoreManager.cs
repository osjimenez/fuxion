namespace Fuxion.EventStore;

using Fuxion.Application.Events;
using Fuxion.Application.Snapshots;
using Fuxion.Domain;
using Fuxion.Json;
using Fuxion.Reflection;
using global::EventStore.Client;
using System.Text;
using System.Text.Json;

public class EventStoreStorage : IEventStorage, ISnapshotStorage
{
	public EventStoreStorage(EventStoreClient client, TypeKeyDirectory typeKeyDirectory)
	{
		this.client = client;
		this.typeKeyDirectory = typeKeyDirectory;
	}

	private readonly EventStoreClient client;
	private readonly TypeKeyDirectory typeKeyDirectory;
	#region IEventStorage
	public async Task CommitAsync(Guid aggregateId, IEnumerable<Event> events)
	{
		var res = await client.AppendToStreamAsync(
			aggregateId.ToString(),
			StreamState.Any,
			events.Select(e =>
			{
				var pod = e.ToEventSourcingPod();
				return new EventData(Uuid.NewUuid(), pod.PayloadKey,JsonSerializer.SerializeToUtf8Bytes(pod));
			}).ToArray());
	}
	public async Task<IQueryable<Event>> GetEventsAsync(Guid aggregateId, int start, int count)
	{
		//TODO - Encontrar una forma de quitar los try-catch solo para detectar que el stream no existe
		try
		{
			//TODO - Implementar un mecanismo de paginación?
			var slice = await client.ReadStreamAsync(Direction.Forwards, aggregateId.ToString(), (ulong)start, count, false)
			.ToListAsync();
			return slice.Select(e =>
				Encoding.Default.GetString(e.Event.Data.ToArray())
				.FromJson<EventSourcingPod>(true)
				.WithTypeKeyDirectory(typeKeyDirectory)).RemoveNulls().AsQueryable();
		}
		catch (StreamNotFoundException)
		{
			return Array.Empty<Event>().AsQueryable();
		}
	}
	public async Task<Event?> GetLastEventAsync(Guid aggregateId)
	{
		try
		{
			var slice = await client.ReadStreamAsync(Direction.Backwards, aggregateId.ToString(), StreamPosition.End, 1, false)
				.ToListAsync();
			return slice.Select(e =>
				Encoding.Default.GetString(e.Event.Data.ToArray())
				.FromJson<EventSourcingPod>(true)
				.WithTypeKeyDirectory(typeKeyDirectory)).LastOrDefault();
		}catch(StreamNotFoundException)
		{
			return null;
		}
	}
	#endregion
	#region ISnapshotStorage
	public async Task<Snapshot?> GetSnapshotAsync(Type snapshotType, Guid aggregateId)
	{
		try
		{
			var slice = await client.ReadStreamAsync(Direction.Backwards, $"{snapshotType.GetTypeKey()}@{aggregateId.ToString()}", StreamPosition.End, 1, false)
			.ToListAsync();
			return slice.Select(e =>
			{
				var pod = Encoding.Default.GetString(e.Event.Data.ToArray()).FromJson<JsonPod<Snapshot, string>>(true);
				return (Snapshot?)pod.As(typeKeyDirectory[pod.PayloadKey]);
			}).LastOrDefault();
		}
		catch (StreamNotFoundException)
		{
			return null;
		}
	}
	public async Task SaveSnapshotAsync(Snapshot snapshot)
	{
		var res = await client.AppendToStreamAsync(
			$"{snapshot.GetType().GetTypeKey()}@{snapshot.AggregateId.ToString()}",
			StreamState.Any,
			new EventData[] {
				new EventData(
					Uuid.NewUuid(),
					snapshot.GetType().GetTypeKey(),
					JsonSerializer.SerializeToUtf8Bytes(new JsonPod<Snapshot, string>(snapshot, snapshot.GetType().GetTypeKey()))
				)
			}
		);
	}
	#endregion
}