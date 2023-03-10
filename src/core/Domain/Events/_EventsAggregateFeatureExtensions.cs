// using Fuxion.Domain.Aggregates;
//
// namespace Fuxion.Domain.Events;
//
// public static class EventsAggregateFeatureExtensions
// {
// 	public static bool HasEvents(this IAggregate me) => me.Features().Has<EventsAggregateFeature>();
// 	public static EventsAggregateFeature Events(this IAggregate me) => me.Features().Get<EventsAggregateFeature>();
// 	public static void AttachEvents(this IAggregate me) => me.AttachFeature(new EventsAggregateFeature());
// 	public static void ApplyEvent(this IAggregate me, Event @event) => me.Events().ApplyEvent(@event);
// 	public static IEnumerable<Event> GetPendingEvents(this IAggregate me) => me.Events().GetPendingEvents();
// 	public static void ClearPendingEvents(this IAggregate me) => me.Events().ClearPendingEvents();
// }