namespace Fuxion.Application.Commands;

using Fuxion.Domain;
using System.Threading.Tasks;

public interface ICommandDispatcher
{
	Task DispatchAsync(Command command);
}