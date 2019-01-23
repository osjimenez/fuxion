using System;
namespace Fuxion.Domain.Events
{
	public class EventFeatureNotFoundException : Exception
	{
		public EventFeatureNotFoundException(string message) : base(message) { }
	}
}
