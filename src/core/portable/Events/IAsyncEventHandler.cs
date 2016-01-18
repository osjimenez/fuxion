using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Events
{
    public interface IAsyncEventHandler
    {
        Task HandleAsync(IEvent @event);
    }
    public interface IAsyncEventHandler<in TEvent> where TEvent : IEvent
    {
        Task HandleAsync(TEvent @event);
    }
}
