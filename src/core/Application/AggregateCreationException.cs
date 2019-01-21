using System;

namespace Fuxion.Application
{
	public class AggregateCreationException : Exception
	{
		public AggregateCreationException(string msg) : base(msg) { }
	}
}
