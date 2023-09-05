namespace Fuxion.Application;

public class AggregateNotFoundException : FuxionException
{
	public AggregateNotFoundException(string msg) : base(msg) { }
}