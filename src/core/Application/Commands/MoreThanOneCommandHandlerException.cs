namespace Fuxion.Application.Commands;

public class MoreThanOneCommandHandlerException : FuxionException
{
	public MoreThanOneCommandHandlerException(string msg) : base(msg) { }
}