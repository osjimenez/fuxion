using Fuxion.Domain;
using Fuxion.Json;

namespace Fuxion.Application.Events;

public class PublicationPodConverter : JsonPodConverter<PublicationPod, Event, string> { }