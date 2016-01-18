using System;
using System.Collections.Generic;
using Fuxion.Events;
using System.Diagnostics;

namespace Fuxion
{
    public abstract class Aggregate : IAggregate, IValidatable
    {
        protected Aggregate(Guid id)
        {
            HandlerRegister reg = new HandlerRegister();
            RegisterHandlers(reg);
            handlers = reg.handlers;
            Version = -1;
            Id = id;
        }
        protected Aggregate(Guid id, IEnumerable<IEvent> history) : this(id) { LoadFrom(history); }

        protected abstract void RegisterHandlers(HandlerRegister register);

        private readonly Dictionary<Type, Action<IEvent>> handlers = new Dictionary<Type, Action<IEvent>>();
        private readonly List<IEvent> _pendingEvents = new List<IEvent>();
        public Guid Id { get; private set; }
        public int Version { get; protected set; }
        public IEnumerable<IEvent> Events { get { return _pendingEvents; } }

        public abstract bool IsValid { get; }
        protected void LoadFrom(IEnumerable<IEvent> pastEvents)
        {
            foreach (var e in pastEvents)
            {
                // TODO - Oscar - Think if aggregate must have a handler for each event type that emit
                if (!handlers.ContainsKey(e.GetType())) continue;
                //throw new HandlerNotFoundException("Handler for type '" + e.GetType().Name + "' not found in aggregate of type '" + GetType().Name + "'.");
                handlers[e.GetType()].Invoke(e);
                Version = e.Version.Value;
            }
        }
        protected void Raise<TEvent>(TEvent @event) where TEvent : IEvent
		{
			@event.SourceId = Id;
			@event.Version = Version + 1;
            var type = @event.GetType();
            if (handlers.ContainsKey(type))
                handlers[type].Invoke(@event);
            Version = @event.Version.Value;
			_pendingEvents.Add(@event);
		}
        public class HandlerRegister
        {
            internal readonly Dictionary<Type, Action<IEvent>> handlers = new Dictionary<Type, Action<IEvent>>();
            public void Handles<TEvent>(Action<TEvent> handler) where TEvent : IEvent { handlers.Add(typeof(TEvent), @event => handler((TEvent)@event)); }
        }
    }
}
