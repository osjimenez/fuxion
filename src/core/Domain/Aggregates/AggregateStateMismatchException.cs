using System;

namespace Fuxion.Domain.Aggregates
{
	public class AggregateStateMismatchException : Exception
	{
		public AggregateStateMismatchException(string message) : base(message) { }
	}
}
