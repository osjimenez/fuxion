namespace Fuxion.Domain;

using Fuxion.Domain.Aggregates;
using Fuxion.Domain.Events;

public abstract class Aggregate
{
	public Guid Id { get; internal set; }
	internal List<IAggregateFeature> Features { get; set; } = new List<IAggregateFeature>();
	protected void ApplyEvent(Event @event) => this.Events().ApplyEvent(@event);
}