using Fuxion.Domain;
using System.Threading.Tasks;

namespace Fuxion.Application.Commands
{
	public interface ICommandDispatcher
	{
		Task DispatchAsync(Command command);
	}
}
