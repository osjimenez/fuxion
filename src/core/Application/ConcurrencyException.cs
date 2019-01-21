using System;

namespace Fuxion.Application
{
	public class ConcurrencyException : Exception
	{
		public ConcurrencyException(string message) : base(message) { }
	}
}
