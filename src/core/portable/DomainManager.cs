using Fuxion.Commands;
using Fuxion.Events;
using Fuxion.Factories;
using Fuxion.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
namespace Fuxion
{
    public static class DomainManager
	{
        #region Events
        // TODO - Oscar - Restore PostSharp
        //[Log(typeof(IEvent), ApplyToStateMachine = true)]
        internal static async Task RaiseAsync(IEvent @event)
        {
            var asyncEventHandlerType = typeof(IAsyncEventHandler<>).MakeGenericType(@event.GetType());
            foreach (var evt in Factory.GetMany(typeof(IAsyncEventHandler<>).MakeGenericType(@event.GetType())))
                await (Task)asyncEventHandlerType.GetRuntimeMethod("HandleAsync", new[] { @event.GetType() }).Invoke(evt, new object[] { @event });

            foreach (var evt in Factory.GetMany<IAsyncEventHandler>())
                await evt.HandleAsync(@event);
        }
        #endregion
        #region Commands
        // TODO - Oscar - Restore PostSharp
        //[Log(typeof(ICommand), ApplyToStateMachine = true)]
        public static async Task DoAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            var async = Factory.GetMany<IAsyncCommandHandler<TCommand>>();
            if (async.Count() != 1) throw new NotImplementedException($"{async.Count()} handlers found for command {typeof(TCommand).Name}, no zero and no many supported, only can be one.");
            await async.Single().HandleAsync(command);
        }
        #endregion
    }
    
}
