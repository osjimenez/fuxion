using Fuxion.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fuxion.Domain.Aggregates
{
	public class EventsAggregateFeature : IAggregateFeature
	{
		Aggregate? aggregate;

		public event EventHandler<EventArgs<Event>>? Applying;
		public event EventHandler<EventArgs<Event>>? Validated;
		public event EventHandler<EventArgs<Event>>? Handled;
		public event EventHandler<EventArgs<Event>>? Pendent;

		public void OnAttach(Aggregate aggregate)
		{
			this.aggregate = aggregate;
			// Setup internal event handlers
			var aggregateType = aggregate.GetType();
			aggregateEventHandlerCache.AddOrUpdate(aggregateType, type =>
				type.GetRuntimeMethods().Where(m =>
					m.ReturnType == typeof(void) &&
					m.GetCustomAttribute<AggregateEventHandlerAttribute>(true) != null &&
					m.GetParameters().Count() == 1 &&
					typeof(Event).IsAssignableFrom(m.GetParameters().First().ParameterType))
					.ToDictionary(m => m.GetParameters().First().ParameterType),
				(_, __) => __);
			eventHandlerCache = aggregateEventHandlerCache[aggregateType].ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		// Events appling
		internal void ApplyEvent(Event @event)
		{
			Applying?.Invoke(this, new EventArgs<Event>(@event));
			Validate(@event);
			Validated?.Invoke(this, new EventArgs<Event>(@event));
			Handle(@event);
			Handled?.Invoke(this, new EventArgs<Event>(@event));
			pendingEvents.Push(@event);
			Pendent?.Invoke(this, new EventArgs<Event>(@event));
		}
		internal void Validate(Event @event)
		{
			if (aggregate?.Id != @event.AggregateId)
				throw new AggregateStateMismatchException($"Aggregate Id is '{aggregate?.Id}' and event.AggregateId is '{@event.AggregateId}'");
		}
		internal void Handle(Event @event)
		{
			if (eventHandlerCache.ContainsKey(@event.GetType()))
				eventHandlerCache[@event.GetType()].Invoke(aggregate, new object[] { @event });
			else throw new AggregateApplyEventMethodMissingException($"No event handler specified for '{@event.GetType()}' on '{GetType()}'");
		}

		// Pending events
		private readonly ConcurrentStack<Event> pendingEvents = new ConcurrentStack<Event>();
		internal bool HasPendingEvents() => !pendingEvents.IsEmpty;
		internal IEnumerable<Event> GetPendingEvents() => pendingEvents.ToArray();
		internal void ClearPendingEvents() => pendingEvents.Clear();
		// Event handlers
		private Dictionary<Type, MethodInfo> eventHandlerCache = new Dictionary<Type, MethodInfo>();
		private static readonly ConcurrentDictionary<Type, Dictionary<Type, MethodInfo>> aggregateEventHandlerCache = new ConcurrentDictionary<Type, Dictionary<Type, MethodInfo>>();
	}
}