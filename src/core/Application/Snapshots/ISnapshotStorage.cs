using Fuxion.Domain;

namespace Fuxion.Application.Snapshots;

public interface ISnapshotStorage
{
	Task<Snapshot?> GetSnapshotAsync(Type      snapshotType, Guid aggregateId);
	Task            SaveSnapshotAsync(Snapshot snapshot);
}

public interface ISnapshotStorage<TAggregate> : ISnapshotStorage where TAggregate : Aggregate { }