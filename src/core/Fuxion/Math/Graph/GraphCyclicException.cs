namespace Fuxion.Math.Graph;

public class GraphCyclicException : FuxionException
{
	public GraphCyclicException() : base() { }
	public GraphCyclicException(string message) : base(message) { }
	public GraphCyclicException(string message, Exception innerException) : base(message, innerException) { }
}