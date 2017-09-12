using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Fuxion.Domain.Events
{
    public interface IEvent
    {
        Guid Id { get; set; }
        long Timestamp { get; set; }
        int? Version { get; set; }
        Guid? SourceId { get; set; }
        //string SourceTypeFullName { get; }
        Guid? SagaId { get; set; }
    }
    public interface IEventBatch : IEvent
    {
        List<IEvent> Events { get; set; }
    }
}

