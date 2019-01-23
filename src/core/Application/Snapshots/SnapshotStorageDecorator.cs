using Fuxion.Domain;
using System;
using System.Threading.Tasks;

namespace Fuxion.Application.Snapshots
{
	internal class SnapshotStorageDecorator<TAggregate> : ISnapshotStorage<TAggregate> where TAggregate : Aggregate, new()
	{
		public SnapshotStorageDecorator(ISnapshotStorage storage)
		{
			this.storage = storage;
		}
		ISnapshotStorage storage;
		public Task<Snapshot> GetSnapshotAsync(Type snapshotType, Guid aggregateId) => storage.GetSnapshotAsync(snapshotType, aggregateId);
		public Task SaveSnapshotAsync(Snapshot snapshot) => storage.SaveSnapshotAsync(snapshot);
	}
}
