using Fuxion.Application.Events;
using Fuxion.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Application.Events
{
	// TODO - Ver si lo puedo quitar, o hacer internal o algo
	public interface IEventPublisher
	{
		Task PublishAsync(Event @event);
	}
}
