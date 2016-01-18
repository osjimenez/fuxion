using Fuxion.Commands;
using Fuxion.Models;
using Fuxion.Events;
using Fuxion.Factories;
using Fuxion.Logging;
using Fuxion.Notifications;
using Fuxion.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public class ApplicationManager
    {
        // TODO - Oscar - Restore PostSharp
        //[Log(typeof(ICommand), ApplyToStateMachine = true)]
        public static Task DoAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            return Factory.Create<IApplicationCommandManager>().DoAsync(command);
        }
        // TODO - Oscar - Restore PostSharp
        //[Log(ApplyToStateMachine = true)]
        public static Task<IDisposable> SubscribeEventAsync<TAggregate, TEvent>(Action<TEvent> action) where TEvent : IEvent
        {
            return Factory.Create<IApplicationEventManager>().SubscribeEventAsync<TAggregate, TEvent>(action);
        }
        // TODO - Oscar - Restore PostSharp
        //[Log(ApplyToStateMachine = true)]
        public static Task<IDisposable> SubscribeNotificationAsync<TNotification>(Action<TNotification> action) 
            where TNotification : INotification
        {
            return Factory.Create<IApplicationNotificationManager>().SubscribeNotificationAsync(action);
        }
        internal static void Raise<TEvent>(TEvent @event) where TEvent : IEvent { }
    }
}
