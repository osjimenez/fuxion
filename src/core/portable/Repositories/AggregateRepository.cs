using Fuxion.Events;
using Fuxion.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Repositories
{
    public abstract class AggregateRepository<TAggregate> where TAggregate : IAggregate
    {
        public abstract Task<TAggregate> FindAsync(Guid id, bool checkIfIsValid = true);
        public abstract Task<TAggregate> GetAsync(Guid id, bool checkIfIsValid = true);
        // TODO - Oscar - Restore PostSharp
        //[Log(typeof(string), typeof(IAggregate), ApplyToStateMachine = true)]
        public async Task SaveAsync(TAggregate aggregate, Guid? sagaId)
        {
            if (!aggregate.Events.Any()) return;
            // TODO - Oscar - Think in correlationId, how will work and if i need it
            //await OnSaveAsync(eventSourced, correlationId);
            await OnSaveAsync(aggregate);
            IEvent @event;
            if (aggregate.Events.Count() > 1) @event = new EventBatch(aggregate.Events);
            else @event = aggregate.Events.Single();
            @event.SagaId = sagaId;
            await DomainManager.RaiseAsync(@event);
        }
        protected abstract Task OnSaveAsync(TAggregate eventSourced);
    }
}
