using System;
using System.Collections.Generic;
using System.Text;

namespace Fuxion.RabbitMQ
{
	public class RabbitMQConnectionException : Exception
	{
		public RabbitMQConnectionException(string message) : base(message) { }
	}
}
