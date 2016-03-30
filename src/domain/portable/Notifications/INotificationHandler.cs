using Fuxion.Domain.Models;
using Fuxion.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Domain.Notifications
{
    public interface IAsyncNotificationHandler
    {
        Task HandleAsync(INotification notification);
    }
}
