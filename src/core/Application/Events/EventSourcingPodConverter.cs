using Fuxion.Domain;
using Fuxion.Json;

namespace Fuxion.Application.Events;

public class EventSourcingPodConverter : JsonPodConverter<EventSourcingPod, Event, string> { }