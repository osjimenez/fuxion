using Fuxion.Domain;
using Fuxion.Reflection;
using Fuxion.Threading;

namespace Fuxion.Application.Events;

public class InMemoryEventStorage : IEventStorage
{
	public InMemoryEventStorage(TypeKeyDirectory typeKeyDirectory, string? dumpFilePath = null)
	{
		if (dumpFilePath != null)
		{
			this.dumpFilePath = new(dumpFilePath);
			this.dumpFilePath.Read(path =>
			{
				if (File.Exists(path))
				{
					var dic = File.ReadAllText(path).FromJson<Dictionary<Guid, List<EventSourcingPod>>>();
					if (dic == null) throw new FileLoadException($"File '{path}' cannot be deserializer for '{nameof(InMemoryEventStorage)}'");
					events.WriteObject(dic.Select(k => (k.Key,
						//Value: k.Value.Select<EventSourcingPod, Event>((EventSourcingPod v) => v.WithTypeKeyDirectory(typeKeyDirectory)).RemoveNulls().ToList<Event>()
						Value: k.Value.Select(v => v.WithTypeKeyDirectory(typeKeyDirectory)).RemoveNulls().ToList())).ToDictionary(a => a.Key, a => a.Value));
				}
			});
		}
	}
	readonly Locker<string>?                       dumpFilePath;
	readonly Locker<Dictionary<Guid, List<Event>>> events = new(new());
	public Task<IQueryable<Event>> GetEventsAsync(Guid aggregateId, int start, int count) =>
		events.ReadAsync(str =>
		{
			// There is no event for this aggregate
			if (!str.ContainsKey(aggregateId)) return new List<Event>().AsQueryable();

			// This is needed for make sure it doesn't fail when we have int.maxValue for count
			if (count > int.MaxValue - start) count = int.MaxValue - start;
			return str[aggregateId].Where(o => str[aggregateId].IndexOf(o) >= start && str[aggregateId].IndexOf(o) < start + count).AsQueryable();
		});
	public Task<Event?> GetLastEventAsync(Guid aggregateId) => events.ReadNullableAsync(str => str.ContainsKey(aggregateId) ? str[aggregateId].Last() : null);
	public async Task CommitAsync(Guid aggregateId, IEnumerable<Event> events)
	{
		if (events.Any())
			await this.events.WriteAsync(str =>
			{
				if (str.ContainsKey(aggregateId) == false)
					str.Add(aggregateId, events.ToList());
				else
					str[aggregateId].AddRange(events);
			});
		if (dumpFilePath != null)
			await dumpFilePath.WriteAsync(path => { this.events.Read(str => File.WriteAllText(path, str.ToDictionary(k => k.Key, k => k.Value.Select(e => e.ToEventSourcingPod())).ToJson())); });
	}
}