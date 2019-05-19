using EventStore.ClientAPI;
using Fuxion.Json;
using Fuxion.Reflection;
using Fuxion.Application.Events;
using Fuxion.Application.Snapshots;
using Fuxion.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.EventStore
{
	public class EventStoreStorage : IEventStorage, ISnapshotStorage
	{
		public EventStoreStorage(IEventStoreConnection connection, TypeKeyDirectory typeKeyDirectory)
		{
			this.connection = connection;
			this.typeKeyDirectory = typeKeyDirectory;
		}

		private readonly IEventStoreConnection connection;
		private readonly TypeKeyDirectory typeKeyDirectory;
		#region IEventStorage
		public async Task CommitAsync(Guid aggregateId, IEnumerable<Event> events)
		{
			var res = await connection.AppendToStreamAsync(
				aggregateId.ToString(),
				ExpectedVersion.Any,
				events.Select(e =>
				{
					var pod = e.ToEventSourcingPod();
					return new EventData(Guid.NewGuid(), pod.PayloadKey, true, Encoding.Default.GetBytes(pod.ToJson()), null);
				}).ToArray());
		}
		public async Task<IQueryable<Event>> GetEventsAsync(Guid aggregateId, int start, int count)
		{
			// TODO - Implementar un mecanismo de paginación cuando tengo que trearme mas de 4096 eventos
			var slice = await connection.ReadStreamEventsForwardAsync(aggregateId.ToString(), start, count == int.MaxValue ? 4096 : count, false);
			return slice.Events.Select(e => Encoding.Default.GetString(e.Event.Data).FromJson<EventSourcingPod>().WithTypeKeyDirectory(typeKeyDirectory)).AsQueryable();
		}
		public async Task<Event?> GetLastEventAsync(Guid aggregateId)
		{
			var slice = await connection.ReadStreamEventsBackwardAsync(aggregateId.ToString(), StreamPosition.End, 1, false);
			return slice.Events.Select(e => Encoding.Default.GetString(e.Event.Data).FromJson<EventSourcingPod>().WithTypeKeyDirectory(typeKeyDirectory)).LastOrDefault();
		}
		#endregion
		#region ISnapshotStorage
		public async Task<Snapshot?> GetSnapshotAsync(Type snapshotType, Guid aggregateId)
		{
			var slice = await connection.ReadStreamEventsBackwardAsync($"{snapshotType.GetTypeKey()}@{aggregateId.ToString()}", StreamPosition.End, 1, false);
			return slice.Events.Select(e =>
			{
				var pod = Encoding.Default.GetString(e.Event.Data).FromJson<JsonPod<Snapshot, string>>();
				return (Snapshot)pod.As(typeKeyDirectory[pod.PayloadKey]);
			}).LastOrDefault();
		}
		public async Task SaveSnapshotAsync(Snapshot snapshot)
		{
			var res = await connection.AppendToStreamAsync(
				$"{snapshot.GetType().GetTypeKey()}@{snapshot.AggregateId.ToString()}",
				ExpectedVersion.Any,
				new EventData(Guid.NewGuid(), snapshot.GetType().GetTypeKey(), true, Encoding.Default.GetBytes(new JsonPod<Snapshot, string>(snapshot, snapshot.GetType().GetTypeKey()).ToJson()), null));
		}
		#endregion
	}
}
