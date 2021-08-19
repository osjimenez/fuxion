namespace Fuxion.Domain.Aggregates;

public class AggregateFeatureNotFoundException : FuxionException
{
	public AggregateFeatureNotFoundException(string message) : base(message) { }
}