using Fuxion.Domain.Models;
using Fuxion.Domain.Events;
using Fuxion.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Domain.Notifications
{
    public interface IAsyncEventProjector
    {
        Task<IEnumerable<INotification>> ProjectsAsync(IEvent @event);
    }
}
