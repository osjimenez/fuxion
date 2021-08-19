namespace Fuxion.Application.Snapshots;

using Fuxion.Domain;

internal class SnapshotStorageDecorator<TAggregate> : ISnapshotStorage<TAggregate> where TAggregate : Aggregate, new()
{
	public SnapshotStorageDecorator(ISnapshotStorage storage) => this.storage = storage;

	private readonly ISnapshotStorage storage;
	public Task<Snapshot?> GetSnapshotAsync(Type snapshotType, Guid aggregateId) => storage.GetSnapshotAsync(snapshotType, aggregateId);
	public Task SaveSnapshotAsync(Snapshot snapshot) => storage.SaveSnapshotAsync(snapshot);
}