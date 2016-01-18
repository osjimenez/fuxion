using Fuxion.Events;
using Fuxion.Factories;
using Fuxion.Logging;
using Fuxion.Notifications;
using Fuxion.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion
{
    public static class ProjectionManager
    {
        // TODO - Oscar - Restore PostSharp
        //[Log(typeof(IEvent), ApplyToStateMachine = true)]
        public static async Task Project<TEvent>(TEvent @event)
            where TEvent : IEvent
            //where TViewModel : IViewModel
        {
            var nots = new List<object>();
            foreach (var evt in Factory.GetAllInstances(typeof(IAsyncEventProjector)))
                nots.AddRange(await ((dynamic)evt).ProjectsAsync(@event));
            foreach (var han in Factory.GetAllInstances(typeof(IAsyncNotificationHandler)))
            {
                foreach (var not in nots)
                {
                    try
                    {
                        await ((dynamic)han).HandleAsync((dynamic)not);
                    }
                    catch (Exception ex){
                        Debug.WriteLine(ex);
                    }
                }
            }
        }
    }
}
