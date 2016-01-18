using Fuxion.Models;
using Fuxion.Events;
using Fuxion.Notifications;
using Fuxion.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public interface IApplicationEventManager
    {
        Task<IDisposable> SubscribeEventAsync<TAggregate, TEvent>(Action<TEvent> action) where TEvent : IEvent;
        //Task<IDisposable> SubscribeNotificationAsync<TNotification>(Action<TNotification> action)
        //    where TNotification : INotification;
    }
}
