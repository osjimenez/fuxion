using Fuxion.Domain.Events;
using System;

namespace Fuxion.Application.Events
{
	public class EventSourcingEventFeature : IEventFeature
	{
		public int TargetVersion { get; internal set; }
		public Guid CorrelationId { get; set; }
		public DateTime EventCommittedTimestamp { get; set; }
		public int ClassVersion { get; set; }
		public bool IsReplay { get; internal set; }
	}
}
