namespace Fuxion.Domain.Aggregates;

public class AggregateStateMismatchException : FuxionException
{
	public AggregateStateMismatchException(string message) : base(message) { }
}