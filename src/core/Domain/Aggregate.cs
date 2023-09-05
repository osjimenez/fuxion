namespace Fuxion.Domain;

// public abstract class Aggregate
// {
// 	public Guid Id { get; internal set; }
// 	internal List<IAggregateFeature> Features { get; set; } = new();
// 	protected void ApplyEvent(Event @event) => this.Events().ApplyEvent(@event);
// }

public interface IAggregate : IFeaturizable<IAggregate>
{
	Guid Id { get; }
}

public static class IAggregateExtensions
{
	public static IFeaturizable<IAggregate> Features(this IAggregate me) => me.Features<IAggregate>();
}