using Fuxion.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Domain
{
    public interface IAggregate : IValidatable
    {
        Guid Id { get; }
        int Version { get; }
        IEnumerable<IEvent> Events { get; }
    }
}
