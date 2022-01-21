namespace Fuxion;

public class InvalidStateException : FuxionException
{
	public InvalidStateException() : base() { }
	public InvalidStateException(string message) : base(message) { }
	public InvalidStateException(string message, Exception innerException) : base(message, innerException) { }
}