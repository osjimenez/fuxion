using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Domain.Events
{
    // TODO - Oscar - Remove EventBatch and mark each event with 'BatchId', 'BatchCount' and 'BatchOrder' properties
    public class EventBatch : IEventBatch
    {
        public EventBatch(IEnumerable<IEvent> events)
        {
            Events = events.ToList();
        }
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<IEvent> Events { get; set; } = new List<IEvent>();
        public Guid? SourceId { get; set; }
        public Guid? SagaId { get; set; }
        public string SourceTypeFullName { get; }
        public long Timestamp { get; set; } = Stopwatch.GetTimestamp();
        public int? Version { get; set; }
    }
}
