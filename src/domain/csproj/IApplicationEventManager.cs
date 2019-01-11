using Fuxion.Domain.Models;
using Fuxion.Domain.Events;
using Fuxion.Domain.Notifications;
using Fuxion.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Domain
{
    public interface IApplicationEventManager
    {
        Task<IDisposable> SubscribeEventAsync<TAggregate, TEvent>(Action<TEvent> action) where TEvent : IEvent;
        //Task<IDisposable> SubscribeNotificationAsync<TNotification>(Action<TNotification> action)
        //    where TNotification : INotification;
    }
}
