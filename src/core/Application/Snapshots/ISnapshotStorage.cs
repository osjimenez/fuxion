namespace Fuxion.Application.Snapshots;

using Fuxion.Domain;
using System;
using System.Threading.Tasks;

public interface ISnapshotStorage
{
	Task<Snapshot?> GetSnapshotAsync(Type snapshotType, Guid aggregateId);
	Task SaveSnapshotAsync(Snapshot snapshot);
}
public interface ISnapshotStorage<TAggregate> : ISnapshotStorage where TAggregate : Aggregate { }