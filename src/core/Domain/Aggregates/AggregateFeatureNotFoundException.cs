using System;

namespace Fuxion.Domain.Aggregates
{
	public class AggregateFeatureNotFoundException : Exception
	{
		public AggregateFeatureNotFoundException(string message) : base(message) { }
	}
}