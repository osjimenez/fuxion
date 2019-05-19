using System;
using System.Collections.Generic;
using System.Text;

namespace Fuxion.RabbitMQ
{
	public class RabbitMQConnectionException : FuxionException
	{
		public RabbitMQConnectionException(string message) : base(message) { }
	}
}
