namespace Fuxion.Application;

using Fuxion.Application.Events;
using Fuxion.Domain;

public interface IEventPublisher<TAggregate> : IEventPublisher where TAggregate : Aggregate { }
