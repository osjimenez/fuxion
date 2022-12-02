using Fuxion.Domain;

namespace Fuxion.Application.Snapshots;

class SnapshotStorageDecorator<TAggregate> : ISnapshotStorage<TAggregate> where TAggregate : Aggregate, new()
{
	public SnapshotStorageDecorator(ISnapshotStorage storage) => this.storage = storage;
	readonly ISnapshotStorage storage;
	public Task<Snapshot?> GetSnapshotAsync(Type snapshotType, Guid aggregateId) => storage.GetSnapshotAsync(snapshotType, aggregateId);
	public Task SaveSnapshotAsync(Snapshot snapshot) => storage.SaveSnapshotAsync(snapshot);
}