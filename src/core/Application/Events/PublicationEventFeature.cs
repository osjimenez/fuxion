using Fuxion.Domain.Events;
using System;

namespace Fuxion.Application.Events
{
	public class PublicationEventFeature : IEventFeature
	{
		public DateTime Timestamp { get; internal set; }
	}
}
