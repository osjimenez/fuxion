using Fuxion.Application.Events;
using Fuxion.Domain;

namespace Fuxion.Application;

public interface IEventPublisher<TAggregate> : IEventPublisher where TAggregate : Aggregate { }
