namespace Fuxion.Domain.Aggregates;

public class AggregateApplyEventMethodMissingException : FuxionException
{
	public AggregateApplyEventMethodMissingException(string message) : base(message) { }
}