namespace Fuxion.Application.Commands;

public class MoreThanOneCommandHandlerException : FuxionException
{
	public MoreThanOneCommandHandlerException(string msg) : base(msg) { }
}

public class NoCommandHandlerFoundException : FuxionException
{
	public NoCommandHandlerFoundException(string msg) : base(msg) { }
}