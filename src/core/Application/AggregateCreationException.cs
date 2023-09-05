namespace Fuxion.Application;

public class AggregateCreationException : FuxionException
{
	public AggregateCreationException(string msg) : base(msg) { }
}