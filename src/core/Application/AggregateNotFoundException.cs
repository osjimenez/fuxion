using System;

namespace Fuxion.Application
{
	public class AggregateNotFoundException : Exception
	{
		public AggregateNotFoundException(string msg) : base(msg) { }
	}
}
