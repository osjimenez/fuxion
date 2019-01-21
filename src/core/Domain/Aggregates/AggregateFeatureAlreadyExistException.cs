using System;

namespace Fuxion.Domain.Aggregates
{
	public class AggregateFeatureAlreadyExistException : Exception
	{
		public AggregateFeatureAlreadyExistException(string message) : base(message) { }
	}
}