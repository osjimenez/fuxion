using System;

namespace Fuxion.Domain
{
	[AttributeUsage(AttributeTargets.Method, Inherited = true)]
	public class AggregateEventHandlerAttribute : Attribute { }
}