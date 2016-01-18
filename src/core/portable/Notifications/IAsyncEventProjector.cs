using Fuxion.Models;
using Fuxion.Events;
using Fuxion.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Notifications
{
    public interface IAsyncEventProjector
    {
        Task<IEnumerable<INotification>> ProjectsAsync(IEvent @event);
    }
}
