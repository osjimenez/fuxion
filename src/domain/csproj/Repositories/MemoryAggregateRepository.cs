using Fuxion.Domain.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Fuxion.Domain.Repositories
{
	public class MemoryAggregateRepository<TAggregate> : AggregateRepository<TAggregate> where TAggregate : class, IAggregate
	{
		public MemoryAggregateRepository()
		{
			_entityFactory = (id, events) => (TAggregate)Activator.CreateInstance(typeof(TAggregate), id, events);
		}

		private readonly List<IEvent> _list = new List<IEvent>();
		private readonly Func<Guid, IEnumerable<IEvent>, TAggregate> _entityFactory;
		public override Task<TAggregate> FindAsync(Guid id, bool checkIfIsValid = true)
		{
			//var evts = _list.Where(evt => evt.SourceId == id);
			//// Hydration
			//var res = _entityFactory(id, evts);
			//if (checkIfIsValid && !res.IsValid) return null;
			//return Task.FromResult(res);
			return Task.FromResult(Find(id, checkIfIsValid));
		}
		public override TAggregate Find(Guid id, bool checkIfIsValid = true)
		{
			IEnumerable<IEvent> evts = _list.Where(evt => evt.SourceId == id);
			// Hydration
			TAggregate res = _entityFactory(id, evts);
			if (checkIfIsValid && !res.IsValid)
			{
				return null;
			}

			return res;
		}
		public override async Task<TAggregate> GetAsync(Guid id, bool checkIfIsValid = true)
		{
			TAggregate entity = await FindAsync(id);
			if (entity == null)
			{
				throw new AggregateNotFoundException(id, typeof(TAggregate).Name);
			}

			return entity;
		}
		protected override Task OnSaveAsync(TAggregate eventSourced)
		{
			Debug.WriteLine($"MemoryEventSourcedRepository<{typeof(TAggregate).Name}>.Save(T eventSourced) - Have {eventSourced.Events.Count()} events to save");
			_list.AddRange(eventSourced.Events);
#if NET45
			return Task.FromResult(0);
#else
			return Task.CompletedTask;
#endif
		}
	}
}
