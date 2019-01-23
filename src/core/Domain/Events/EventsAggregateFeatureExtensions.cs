using Fuxion.Domain.Aggregates;
using Fuxion.Domain;
using System.Collections.Generic;

namespace Fuxion.Domain.Events
{
	public static class EventsAggregateFeatureExtensions
	{
		public static bool HasEvents(this Aggregate me) => me.HasFeature<EventsAggregateFeature>();
		public static EventsAggregateFeature Events(this Aggregate me) => me.GetFeature<EventsAggregateFeature>();
		public static void AttachEvents(this Aggregate me) => me.AttachFeature(new EventsAggregateFeature());
		public static void ApplyEvent(this Aggregate me, Event @event) => me.Events().ApplyEvent(@event);
		public static IEnumerable<Event> GetPendingEvents(this Aggregate me) => me.Events().GetPendingEvents();
		public static void ClearPendingEvents(this Aggregate me) => me.Events().ClearPendingEvents();
	}
}
