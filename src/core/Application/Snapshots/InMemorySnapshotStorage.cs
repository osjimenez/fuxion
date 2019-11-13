using Fuxion.Json;
using Fuxion.Reflection;
using Fuxion.Repositories;
using Fuxion.Domain;
using Fuxion.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace Fuxion.Application.Snapshots
{
	public class InMemorySnapshotStorage : ISnapshotStorage
	{
		public InMemorySnapshotStorage(TypeKeyDirectory typeKeyDirectory, string? dumpFilePath = null)
		{
			if (dumpFilePath != null)
			{
				this.dumpFilePath = new Locker<string>(dumpFilePath);
				this.dumpFilePath.Read(path =>
				{
					if (File.Exists(path))
					{
						var dic = File.ReadAllText(path).FromJson<Dictionary<string, List<JsonPod<Snapshot, string>>>>();
						snapshots.WriteObject(dic.Select((KeyValuePair<string, List<JsonPod<Snapshot, string>>> k) =>
						(
							Key: k.Key,
							Value: k.Value.Select((JsonPod<Snapshot, string> v) => (Snapshot?)v.As(typeKeyDirectory[k.Key])).RemoveNulls().ToDictionary((Snapshot s) => s.AggregateId)
						)).ToDictionary(a => a.Key, a => a.Value));
					}
				});
			}
		}
		
		private readonly Locker<string>? dumpFilePath = null;
		private readonly Locker<Dictionary<string, Dictionary<Guid, Snapshot>>> snapshots = new Locker<Dictionary<string, Dictionary<Guid, Snapshot>>>(new Dictionary<string, Dictionary<Guid, Snapshot>>());

		public Task<Snapshot?> GetSnapshotAsync(Type snapshotType, Guid aggregateId) => snapshots.ReadAsync(dic =>
			dic.ContainsKey(snapshotType.GetTypeKey()) && dic[snapshotType.GetTypeKey()].ContainsKey(aggregateId)
				? (Snapshot)dic[snapshotType.GetTypeKey()][aggregateId]
				: null);

		public async Task SaveSnapshotAsync(Snapshot snapshot)
		{
			var key = snapshot.GetType().GetTypeKey();
			await snapshots.WriteAsync(dic =>
			{
				if (!dic.ContainsKey(key))
					dic.Add(key, new Dictionary<Guid, Snapshot>());
				if (dic[key].ContainsKey(snapshot.AggregateId))
					dic[key][snapshot.AggregateId] = snapshot;
				else
					dic[key].Add(snapshot.AggregateId, snapshot);
			});
			if (dumpFilePath != null)
				await dumpFilePath.WriteAsync(path =>
				{
					snapshots.Read(str => File.WriteAllText(
						path,
						str.ToDictionary(
							k => k.Value.First().Value.GetType().GetTypeKey(),
							k => k.Value.Select(e => new JsonPod<Snapshot, string>(e.Value, e.Value.GetType().GetTypeKey()))
						).ToJson()));
				});
		}
	}
}
