namespace Fuxion.Application.Events;

using Fuxion.Domain;
using Fuxion.Json;

public class EventSourcingPodConverter : JsonPodConverter<EventSourcingPod, Event, string>
{

}