using System;
using System.Collections.Generic;
using System.Text;

namespace Fuxion.Domain.Events
{
	public class EventFeatureAlreadyExistException : FuxionException
	{
		public EventFeatureAlreadyExistException(string message) : base(message) { }
	}
}
