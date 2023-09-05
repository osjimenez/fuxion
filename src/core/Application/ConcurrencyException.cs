namespace Fuxion.Application;

public class ConcurrencyException : FuxionException
{
	public ConcurrencyException(string message) : base(message) { }
}