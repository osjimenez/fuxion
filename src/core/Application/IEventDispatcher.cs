using Fuxion.Domain;
using System.Threading.Tasks;
namespace Fuxion.Application
{
	public interface IEventDispatcher
	{
		Task DispatchAsync(Event @event);
	}
}