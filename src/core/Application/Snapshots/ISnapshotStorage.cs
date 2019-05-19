using Fuxion.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Application.Snapshots
{
	public interface ISnapshotStorage
	{
		Task<Snapshot?> GetSnapshotAsync(Type snapshotType, Guid aggregateId);
		Task SaveSnapshotAsync(Snapshot snapshot);
	}
	public interface ISnapshotStorage<TAggregate> : ISnapshotStorage where TAggregate : Aggregate { }
}
