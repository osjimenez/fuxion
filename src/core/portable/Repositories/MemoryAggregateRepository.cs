using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fuxion;
using Fuxion.Events;
using Fuxion.Factories;
using Fuxion.Repositories;

namespace Fuxion.Repositories
{
    public class MemoryAggregateRepository<TAggregate> : AggregateRepository<TAggregate> where TAggregate : class, IAggregate
    {
        public MemoryAggregateRepository()
        {
            _entityFactory = (id, events) => (TAggregate)Activator.CreateInstance(typeof(TAggregate), id, events);
        }
        readonly List<IEvent> _list = new List<IEvent>();
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
            var evts = _list.Where(evt => evt.SourceId == id);
            // Hydration
            var res = _entityFactory(id, evts);
            if (checkIfIsValid && !res.IsValid) return null;
            return res;
        }
        public override async Task<TAggregate> GetAsync(Guid id, bool checkIfIsValid = true)
        {
            var entity = await FindAsync(id);
            if (entity == null)
                throw new AggregateNotFoundException(id, typeof(TAggregate).Name);
            return entity;
        }
        protected override Task OnSaveAsync(TAggregate eventSourced)
        {
            Debug.WriteLine($"MemoryEventSourcedRepository<{typeof(TAggregate).Name}>.Save(T eventSourced) - Have {eventSourced.Events.Count()} events to save");
            _list.AddRange(eventSourced.Events);
            return Task.FromResult(0);
        }
    }
}
