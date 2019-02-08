using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fuxion.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Fuxion.Application
{
	public interface IEventHandler<in TEvent> where TEvent : Event
	{
		Task HandleAsync(TEvent @event);
	}
}
