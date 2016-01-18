using Fuxion.Models;
using Fuxion.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Notifications
{
    public interface IAsyncNotificationHandler
    {
        Task HandleAsync(INotification notification);
    }
}
