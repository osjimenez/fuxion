using Fuxion.Domain;
using Fuxion.Json;
using Fuxion.Reflection;

namespace Fuxion.Application.Events;

public class EventSourcingPodConverter : JsonPodConverter<EventSourcingPod, TypeKey, Event> { }